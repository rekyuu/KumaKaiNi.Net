using System.Threading.Tasks;

namespace KumaKaiNi.Twitch
{
    public class Program
    {
        public static async Task Main()
        {
            App twitch = new App();
            await twitch.Start();
        }
    }
}
