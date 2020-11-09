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
            Database.CreateTable<Log>();
            Database.CreateTable<Quote>();
            Database.CreateTable<CustomCommand>();

            return new Response("That probably worked.");
        }

        [Command("drop")]
        public static Response DropDatabase()
        {
            Database.DropTable<Log>();
            Database.DropTable<Quote>();
            Database.DropTable<CustomCommand>();

            return new Response("Everyone is dead.");
        }

        [Command("migrate")]
        public static Response Migrate()
        {
            return new Response();
        }
    }
}
