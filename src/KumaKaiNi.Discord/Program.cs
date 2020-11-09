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
        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            App discord = new App();
            await discord.Start();
        }
    }
}
