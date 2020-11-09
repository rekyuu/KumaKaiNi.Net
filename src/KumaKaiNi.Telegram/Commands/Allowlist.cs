using KumaKaiNi.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KumaKaiNi.Telegram
{
    [RequireAdmin]
    public static class Allowlist
    {
        [Command("initallowlist")]
        public static Response InitWhitelist()
        {
            Database.DropTable<TelegramAllowlist>();
            Database.CreateTable<TelegramAllowlist>();

            return new Response("Done.");
        }

        [Command("approve")]
        public static Response AddToWhitelist(Request request)
        {
            List<TelegramAllowlist> whitelist = Database.GetMany<TelegramAllowlist>();

            foreach (TelegramAllowlist entry in whitelist)
            {
                if (entry.ChannelId == request.ChannelId)
                {
                    entry.Approved = true;
                    entry.Warnings = 0;
                    entry.Update();
                }
            }

            return new Response("Added to whitelist.");
        }
    }
}
