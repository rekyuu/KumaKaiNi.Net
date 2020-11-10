using System.Threading.Tasks;

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
