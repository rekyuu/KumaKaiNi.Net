﻿using KumaKaiNi.Core;
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
        public static Response InitAllowlist()
        {
            Database.DropTable<TelegramAllowlist>();
            Database.CreateTable<TelegramAllowlist>();

            return new Response("Done.");
        }

        [Command("approve")]
        public static Response AddToAllowlist(Request request)
        {
            List<TelegramAllowlist> allowlist = Database.GetMany<TelegramAllowlist>();

            foreach (TelegramAllowlist entry in allowlist)
            {
                if (entry.ChannelId == request.ChannelId)
                {
                    entry.Approved = true;
                    entry.Warnings = 0;
                    entry.Update();

                    break;
                }
            }

            return new Response("Added to allowlist.");
        }

        [Command("leave")]
        public static Response LeaveChat()
        {
            return new Response()
            {
                AdminMessage = "LEAVE_CHAT"
            };
        }
    }
}
