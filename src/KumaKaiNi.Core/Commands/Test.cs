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
            List<Log> response = Database.GetResults<Log>();
            return new Response(response[^1].Message);
        }
    }
}
