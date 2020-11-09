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
        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            App twitch = new App();
            await twitch.Start();
        }
    }
}
