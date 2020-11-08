using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Test
    {
        [Command("test")]
        public static Response TestCommand()
        {
            string response = Database.GetVersion();
            return new Response(response);
        }
    }
}
