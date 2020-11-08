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

        public static void Main(string[] args)
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
            Request request = new Request(message.ChatMessage.Message);
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") _twitch.SendMessage(message.ChatMessage.Channel, response.Message);
        }

        private void WhisperReceived(object sender, OnWhisperReceivedArgs message)
        {
            Request request = new Request(message.WhisperMessage.Message);
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") _twitch.SendWhisper(message.WhisperMessage.Username, response.Message);
        }
    }
}
