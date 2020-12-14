using Discord;
using Discord.WebSocket;
using KumaKaiNi.Core;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace KumaKaiNi.Discord
{
    public class App
    {
        private readonly KumaClient _kuma;
        private readonly DiscordSocketClient _discord;

        private int _currentMoonPhase = -1;
        private int _currentMonth = -1;
        private bool _avatarIsFestive = false;

        public App()
        {
            _kuma = new KumaClient();
            _discord = new DiscordSocketClient();

            _discord.Log += Log;
            _discord.Ready += OnReady;
            _discord.MessageReceived += MessageReceived;
        }

        public async Task Start()
        {
            string token = ConfigurationManager.AppSettings.Get("DiscordToken");

            await _discord.LoginAsync(TokenType.Bot, token);
            await _discord.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private Task MessageReceived(SocketMessage message)
        {
            try
            {
                if (message.Author.IsWebhook) return Task.CompletedTask;
                if (message.Author.Id == _discord.CurrentUser.Id) return Task.CompletedTask;

                SocketTextChannel channel = (SocketTextChannel)message.Channel;

                bool isModerator = false;
                foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
                {
                    if (role.Id.ToString() == ConfigurationManager.AppSettings.Get("DiscordModeratorRoleID")) isModerator = true;
                }

                bool isAdmin = message.Author.Id.ToString() == ConfigurationManager.AppSettings.Get("DiscordAdminID");
                bool isPrivate = channel == null;
                bool isNsfw = isPrivate || channel.IsNsfw;

                UserAuthority authority = UserAuthority.User;
                if (isAdmin) authority = UserAuthority.Admin;
                else if (isModerator) authority = UserAuthority.Moderator;

                Request request = new Request()
                {
                    Message = message.Content,
                    MessageId = (long)message.Id,
                    Username = message.Author.Username,
                    Authority = authority,
                    Protocol = RequestProtocol.Discord,
                    ChannelId = (long)channel.Id,
                    ChannelIsPrivate = isPrivate,
                    ChannelIsNSFW = isNsfw,
                };

                using (message.Channel.EnterTypingState())
                {
                    Response response = _kuma.GetResponse(request);

                    if (response.Message != "")
                    {
                        message.Channel.SendMessageAsync(response.Message);
                    }
                    else if (response.Image != null)
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(0x00b6b6),
                            Title = response.Image.Referrer,
                            Url = response.Image.Source,
                            Description = response.Image.Description,
                            ImageUrl = response.Image.URL,
                            Timestamp = DateTime.UtcNow
                        };

                        message.Channel.SendMessageAsync(text: response.Message, embed: embed.Build());
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }

            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            UpdateMoonPhase();

            Timer moonTimer = new Timer(60 * 60 * 1000);
            moonTimer.Elapsed += new ElapsedEventHandler(MoonPhaseHandler);
            moonTimer.Start();

            UpdateFestiveAvatar();

            Timer festiveTimer = new Timer(24 * 60 * 60 * 1000);
            festiveTimer.Elapsed += new ElapsedEventHandler(FestiveAvatarHandler);
            festiveTimer.Start();

            return Task.CompletedTask;
        }

        private void MoonPhaseHandler(object source, ElapsedEventArgs e)
        {
            UpdateMoonPhase();
        }

        private void UpdateMoonPhase()
        {
            try
            {
                int phase = Helpers.GetMoonPhase(DateTime.UtcNow);
                if (phase == _currentMoonPhase) return;
                _currentMoonPhase = phase;

                Console.WriteLine($"{DateTime.UtcNow} [KumaKaiNi.Discord] Updating moon phase: {_currentMoonPhase}");

                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream($"KumaKaiNi.Discord.Resources.Phase{phase}.jpg");
                using Image currentPhase = new Image(stream);

                SocketGuild guild = _discord.GetGuild(214268737887404042);
                guild.ModifyAsync(delegate (GuildProperties p) { p.Icon = currentPhase; });
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }

        private void FestiveAvatarHandler(object source, ElapsedEventArgs e)
        {
            UpdateFestiveAvatar();
        }

        private void UpdateFestiveAvatar()
        {
            try
            {
                _currentMonth = DateTime.UtcNow.Month;

                if (_currentMonth == 12 && _avatarIsFestive || _currentMonth != 12 && !_avatarIsFestive) return;
                else
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Stream stream;

                    if (_currentMonth == 12 && !_avatarIsFestive)
                    {
                        stream = assembly.GetManifestResourceStream($"KumaKaiNi.Discord.Resources.KumaFestive.png");
                        _avatarIsFestive = true;
                    }
                    else
                    {
                        stream = assembly.GetManifestResourceStream($"KumaKaiNi.Discord.Resources.KumaStandard.png");
                        _avatarIsFestive = false;
                    }

                    string avatarLogString = _avatarIsFestive ? "Festive" : "Standard";
                    Console.WriteLine($"{DateTime.UtcNow} [KumaKaiNi.Discord] Updating avatar: {avatarLogString}");

                    using Image avatar = new Image(stream);
                    _discord.CurrentUser.ModifyAsync(delegate (SelfUserProperties p) { p.Avatar = avatar; });
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }
    }
}
