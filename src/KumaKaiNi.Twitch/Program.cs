using KumaKaiNi.Core;
using System;
using System.Threading.Tasks;
using System.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace KumaKaiNi.Twitch
{
    class Program
    {
        private KumaClient _kuma;
        private TwitchClient _twitch;

        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _kuma = new KumaClient();

            ConnectionCredentials credentials = new ConnectionCredentials("KumaKaiNi", ConfigurationManager.AppSettings.Get("TwitchAccessToken"));
            ClientOptions clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
            _twitch = new TwitchClient(webSocketClient);
            _twitch.Initialize(credentials, "rekyuus");

            _twitch.OnLog += Log;
            _twitch.OnConnected += Connected;
            _twitch.OnJoinedChannel += ChannelJoined;
            _twitch.OnMessageReceived += MessageReceived;
            _twitch.OnWhisperReceived += WhisperReceived;

            _twitch.Connect();

            await Task.Delay(-1);
        }

        private void Log(object sender, OnLogArgs e)
        {
            // Console.WriteLine($"[{DateTime.UtcNow}] {e.BotUsername}: {e.Data}");
        }

        private void Connected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Connected to {e.AutoJoinChannel}");
        }

        private void ChannelJoined(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Joined {e.Channel}");
        }

        private void MessageReceived(object sender, OnMessageReceivedArgs message)
        {
            bool isAdmin = message.ChatMessage.Username == "rekyuus";
            bool isModerator = message.ChatMessage.IsModerator;

            UserAuthority authority = UserAuthority.User;
            if (isAdmin) authority = UserAuthority.Admin;
            else if (isModerator) authority = UserAuthority.Moderator;

            Request request = new Request()
            {
                Message = message.ChatMessage.Message,
                MessageId = 0,
                Username = message.ChatMessage.Username,
                Authority = authority,
                Protocol = RequestProtocol.Twitch,
                ChannelId = 0,
                ChannelIsPrivate = false,
                ChannelIsNSFW = false,
            };
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") _twitch.SendMessage(message.ChatMessage.Channel, response.Message);
        }

        private void WhisperReceived(object sender, OnWhisperReceivedArgs message)
        {
            bool isAdmin = message.WhisperMessage.Username == "rekyuus";

            UserAuthority authority = UserAuthority.User;
            if (isAdmin) authority = UserAuthority.Admin;

            Request request = new Request()
            {
                Message = message.WhisperMessage.Message,
                MessageId = 0,
                Username = message.WhisperMessage.Username,
                Authority = authority,
                Protocol = RequestProtocol.Twitch,
                ChannelId = 0,
                ChannelIsPrivate = true,
                ChannelIsNSFW = false,
            };
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") _twitch.SendWhisper(message.WhisperMessage.Username, response.Message);
        }
    }
}
