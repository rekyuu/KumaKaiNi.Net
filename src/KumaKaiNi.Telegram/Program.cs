using System.Threading.Tasks;

namespace KumaKaiNi.Telegram
{
    class Program
    {
        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            App telegram = new App();
            await telegram.Start();
        }
    }
}
