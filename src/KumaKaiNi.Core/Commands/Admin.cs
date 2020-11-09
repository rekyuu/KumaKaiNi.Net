using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Admin
    {
        [Command("init")]
        public static Response InitDatabase()
        {
            Database.Init();
            return new Response("That probably worked.");
        }

        [Command("migrate")]
        public static Response Migrate()
        {
            return new Response();
        }
    }
}
