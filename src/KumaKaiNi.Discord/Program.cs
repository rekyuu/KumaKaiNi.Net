using Discord;
using Discord.WebSocket;
using KumaKaiNi.Core;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace KumaKaiNi.Discord
{
    class Program
    {
        private KumaClient _kuma;
        private DiscordSocketClient _discord;

        public static void Main(string[] args)
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
            Request request = new Request(message.Content);
            Response response = _kuma.GetResponse(request);

            if (response.Message != "") message.Channel.SendMessageAsync(response.Message);

            return Task.CompletedTask;
        }
    }
}
