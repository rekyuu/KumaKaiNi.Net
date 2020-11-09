using Discord;
using Discord.WebSocket;
using KumaKaiNi.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace KumaKaiNi.Discord
{
    class Program
    {
        private KumaClient _kuma;
        private DiscordSocketClient _discord;

        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _kuma = new KumaClient();

            _discord = new DiscordSocketClient();
            _discord.Log += Log;
            _discord.MessageReceived += MessageReceived;

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
            SocketTextChannel channel = (SocketTextChannel)message.Channel;

            bool isModerator = false;
            foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
            {
                if (role.Id.ToString() == ConfigurationManager.AppSettings.Get("ModeratorRoleID")) isModerator = true;
            }

            bool isAdmin = message.Author.Id.ToString() == ConfigurationManager.AppSettings.Get("AdminID");
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
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") message.Channel.SendMessageAsync(response.Message);

            return Task.CompletedTask;
        }
    }
}
