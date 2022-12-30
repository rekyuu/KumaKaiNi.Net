using System.Threading.Tasks;

namespace KumaKaiNi.Discord
{
    public class Program
    {
        public static async Task Main()
        {
            App discord = new App();
            await discord.Start();
        }
    }
}
