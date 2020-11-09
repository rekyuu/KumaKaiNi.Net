﻿using KumaKaiNi.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace KumaKaiNi.Telegram
{
    class Program
    {
        private static KumaClient _kuma;
        private static ITelegramBotClient _telegram;

        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _kuma = new KumaClient();
            _telegram = new TelegramBotClient(ConfigurationManager.AppSettings.Get("TelegramToken"));

            _telegram.OnMessage += OnMessage;
            _telegram.StartReceiving();

            await Task.Delay(-1);
        }

        static async void OnMessage(object sender, MessageEventArgs e)
        {
            bool isAdmin = e.Message.From.Id == int.Parse(ConfigurationManager.AppSettings.Get("TelegramAdminId"));
            List<TelegramAllowlist> whitelist = Database.GetMany<TelegramAllowlist>();

            TelegramAllowlist whitelistEntry = null;
            foreach (TelegramAllowlist entry in whitelist)
            {
                if (entry.ChannelId == e.Message.Chat.Id)
                {
                    whitelistEntry = entry;
                    break;
                }
            }

            if (whitelistEntry == null)
            {
                whitelistEntry = new TelegramAllowlist()
                {
                    ChannelId = e.Message.Chat.Id,
                    Approved = false,
                    Warnings = 0
                };

                whitelistEntry.Insert();
            }

            if (!whitelistEntry.Approved && !isAdmin)
            {
                if (whitelistEntry.Warnings >= 5)
                {
                    await _telegram.LeaveChatAsync(e.Message.Chat.Id);
                    return;
                }
                else
                {
                    whitelistEntry.Warnings++;
                    whitelistEntry.Update();
                    return;
                }
            }

            UserAuthority authority = UserAuthority.User;
            if (isAdmin) authority = UserAuthority.Admin;

            string message = e.Message.Text ?? "";
            if (message != "")
            {
                if (message[0] == '/') message = '!' + message[1..];
                message = message.Replace("@KumaBot", "");
            }

            Request request = new Request()
            {
                Message = message,
                MessageId = e.Message.MessageId,
                Username = e.Message.From.FirstName + (e.Message.From.LastName == "" ? "" : " " + e.Message.From.LastName),
                Authority = authority,
                Protocol = RequestProtocol.Telegram,
                ChannelId = e.Message.Chat.Id,
                ChannelIsPrivate = e.Message.Chat.Id == e.Message.From.Id,
                ChannelIsNSFW = true,
            };
            Response response = _kuma.GetResponse(request);

            if (response.Message != "")
            {
                await _telegram.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: response.Message);
            }
        }
    }
}