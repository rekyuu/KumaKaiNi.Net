using System.Threading.Tasks;

namespace KumaKaiNi.Telegram
{
    public class Program
    {
        public static async Task Main()
        {
            App telegram = new App();
            await telegram.Start();
        }
    }
}
