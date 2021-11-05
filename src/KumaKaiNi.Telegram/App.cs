using KumaKaiNi.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace KumaKaiNi.Telegram
{
    public class App
    {
        private static KumaClient _kuma;
        private static ITelegramBotClient _telegram;

        public App()
        {
            _kuma = new KumaClient();
            _telegram = new TelegramBotClient(ConfigurationManager.AppSettings.Get("TelegramToken"));

            _telegram.OnMessage += OnMessage;
        }

        public async Task Start()
        {
            _telegram.StartReceiving();

            await Task.Delay(-1);
        }

        static async void OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.From.Id == _telegram.BotId) return;

                bool isAdmin = e.Message.From.Id == int.Parse(ConfigurationManager.AppSettings.Get("TelegramAdminId"));
                bool isPrivate = e.Message.Chat.Id == e.Message.From.Id;

                if (!isAdmin && isPrivate) return;

                List<TelegramAllowlist> allowlist = Database.GetMany<TelegramAllowlist>();

                TelegramAllowlist allowlistEntry = null;
                foreach (TelegramAllowlist entry in allowlist)
                {
                    if (entry.ChannelId == e.Message.Chat.Id)
                    {
                        allowlistEntry = entry;
                        break;
                    }
                }

                if (allowlistEntry == null)
                {
                    allowlistEntry = new TelegramAllowlist()
                    {
                        ChannelId = e.Message.Chat.Id,
                        Approved = false,
                        Warnings = 0
                    };

                    allowlistEntry.Insert();
                }

                if (!allowlistEntry.Approved && !isAdmin)
                {
                    if (allowlistEntry.Warnings >= 5)
                    {
                        try
                        {
                            await _telegram.LeaveChatAsync(e.Message.Chat.Id);
                        }
                        catch { }
                        return;
                    }
                    else
                    {
                        allowlistEntry.Warnings++;
                        allowlistEntry.Update();
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
                    Username = e.Message.From.FirstName + (e.Message.From.LastName == null ? "" : " " + e.Message.From.LastName),
                    Authority = authority,
                    Protocol = RequestProtocol.Telegram,
                    ChannelId = e.Message.Chat.Id,
                    ChannelIsPrivate = isPrivate,
                    ChannelIsNsfw = true,
                };

                Response response = _kuma.GetResponse(request);

                if (response.AdminMessage == "LEAVE_CHAT")
                {
                    await _telegram.LeaveChatAsync(e.Message.Chat.Id);
                }
                else if (response.Message != "")
                {
                    _ = _telegram.SendChatActionAsync(chatId: e.Message.Chat.Id, chatAction: ChatAction.Typing);
                    await _telegram.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: response.Message);
                }
                else if (response.Image != null)
                {
                    _ = _telegram.SendChatActionAsync(chatId: e.Message.Chat.Id, chatAction: ChatAction.Typing);

                    string caption = $"{response.Image.Description}";
                    if (response.Image.Referrer != "" && response.Image.Source != "") caption += $"\n\n[{response.Image.Referrer}]({response.Image.Source})";

                    try
                    {
                        await _telegram.SendPhotoAsync(chatId: e.Message.Chat.Id, photo: response.Image.Url, caption: caption, parseMode: ParseMode.Markdown);
                    }
                    catch
                    {
                        await _telegram.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: $"Image was too large for telegram.\n\n[{response.Image.Referrer}]({response.Image.Source})\n\n{response.Image.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }
    }
}
