using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    [RequireAdmin]
    public static class Test
    {
        [Command("lastlog")]
        public static Response LastLog()
        {
            List<Log> response = Database.GetMany<Log>();
            return new Response(response[^1].Message);
        }

        [Command("test")]
        public static Response CreateTable()
        {
            return new Response("done");
        }
    }
}
