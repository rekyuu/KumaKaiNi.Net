using KumaKaiNi.Core;
using System;

namespace KumaKaiNi.DevConsole
{
    class Program
    {
        private static KumaClient _kuma;

        static void Main()
        {
            _kuma = new KumaClient();

            while (true)
            {
                Console.Write("> ");
                string message = Console.ReadLine();

                if (message == "exit") Environment.Exit(0);

                try
                {
                    Request request = new Request()
                    {
                        Message = message,
                        MessageId = 0,
                        Username = "rekyuu",
                        Authority = UserAuthority.Admin,
                        Protocol = RequestProtocol.Terminal,
                        ChannelId = 0,
                        ChannelIsPrivate = true,
                        ChannelIsNSFW = true,
                    };
                    Response response = _kuma.GetResponse(request);

                    if (response.Message != "") Console.WriteLine($"\n  {response.Message}\n");
                    else if (response.Image != null) Console.WriteLine($"\n  {response.Image.URL}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n  ERROR: {ex}\n");
                }
            }
        }
    }
}
