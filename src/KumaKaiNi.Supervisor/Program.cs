using System;
using System.Threading.Tasks;

namespace KumaKaiNi.Supervisor
{
    class Program
    {
        private Discord.App _discord;
        private Telegram.App _telegram;
        private Twitch.App _twitch;

        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _discord = new Discord.App();
            _telegram = new Telegram.App();
            _twitch = new Twitch.App();

            _ = _discord.Start();
            _ = _telegram.Start();
            _ = _twitch.Start();

            await Task.Delay(-1);
        }
    }
}
