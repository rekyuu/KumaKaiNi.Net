using System.Text.Json;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KumaKaiNi.Telegram;

internal static class Program
{
    private static string ConsumerStreamName => Redis.GetStreamNameForSourceSystem(SourceSystem.Telegram);
    
    private static RedisStreamConsumer? _streamConsumer;
    private static ITelegramBotClient? _telegramClient;
    private static CancellationTokenSource? _cts;
    private static string? _botUsername;
    
    private static async Task Main()
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            if (string.IsNullOrEmpty(KumaTelegramConfig.TelegramAccessToken))
            {
                Log.Fatal("TELEGRAM_ACCESS_TOKEN environment variable must be set, exiting");
                Environment.Exit(1);
            }

            _cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                _cts.Cancel();
                eventArgs.Cancel = true;
            };
            
            _streamConsumer = new RedisStreamConsumer(
                ConsumerStreamName,
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntriesReceived += OnStreamEntriesReceived;
            await _streamConsumer.StartAsync();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = [] // receive all update types except ChatMember related updates
            };

            _telegramClient = new TelegramBotClient(KumaTelegramConfig.TelegramAccessToken);
            User me = await _telegramClient.GetMeAsync();
            _botUsername = me.Username;
            Log.Information("Logged in as @{Username}", _botUsername);

            _telegramClient.StartReceiving(
                updateHandler: HandleTelegramUpdateAsync,
                pollingErrorHandler: HandleTelegramPollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _cts.Token);

            Log.Information("Listening for updates");
            await Task.Delay(-1, _cts.Token);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Exiting");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "An exception was thrown while starting");
            Environment.Exit(1);
        }
    }

    private static async Task HandleTelegramUpdateAsync(ITelegramBotClient telegramClient, Update update, CancellationToken ct)
    {
        // Skip update if no message exists or messages from self
        if (update.Message is not { } message) return;
        if (message.From?.Id == telegramClient.BotId) return;

        // Determine user authority and if the chat is a private chat
        bool isAdmin = message.From?.Id == KumaTelegramConfig.TelegramAdminId;
        bool isPrivate = message.Chat.Id == message.From?.Id;
        UserAuthority authority = isAdmin ? UserAuthority.Administrator : UserAuthority.User;

        // Skip if it's a private chat that isn't from the administrator
        if (!isAdmin && isPrivate) return;

        // Determine if the chat is allowed
        await using KumaKaiNiDbContext db = new();
        TelegramAllowList? allowList = await db.TelegramAllowList
            .Where(x => x.ChannelId == message.Chat.Id)
            .FirstOrDefaultAsync(ct);

        if (allowList == null)
        {
            allowList = new TelegramAllowList
            {
                ChannelId = message.Chat.Id,
                Approved = false,
                Warnings = 0
            };

            await db.TelegramAllowList.AddAsync(allowList, ct);
        }

        // If the chat isn't whitelisted yet, leave after 5 messages
        if (!allowList.Approved && !isAdmin)
        {
            if (allowList.Warnings >= 5)
            {
                try
                {
                    await telegramClient.LeaveChatAsync(message.Chat.Id, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await Logging.LogExceptionToDatabaseAsync(ex, "Exception was thrown while trying to leave chat {ChatId}", message.Chat.Id);
                }
                
                return;
            }

            allowList.Warnings++;
            return;
        }

        await db.SaveChangesAsync(ct);
        
        // Update request message so that slash commands start with ! instead
        string? requestMessage = message.Text;
        if (!string.IsNullOrEmpty(requestMessage))
        {
            if (requestMessage[0] == '/') requestMessage = '!' + requestMessage[1..];
            requestMessage = requestMessage.Replace($"@{_botUsername}", "");
        }
        else return;

        // Send the request
        KumaRequest kumaRequest = new(
            message.From?.FirstName + (string.IsNullOrEmpty(message.From?.LastName) ? "" : " " + message.From.LastName),
            requestMessage,
            SourceSystem.Telegram,
            message.MessageId,
            authority,
            message.Chat.Id,
            isPrivate,
            true);

        await Redis.AddRequestToStream(kumaRequest);
    }

    private static Task HandleTelegramPollingErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
    {
        Log.Error(ex, "An exception was thrown while polling Telegram");
        return Task.CompletedTask;
    }

    private static void OnKumaProcessing(long? channelId)
    {
        if (_telegramClient == null) return;
        if (channelId == null) return;
        
        _ = _telegramClient.SendChatActionAsync(chatId: channelId, chatAction: ChatAction.Typing);
    }

    private static async void OnStreamEntriesReceived(StreamEntry[] streamEntries)
    {
        if (_telegramClient == null) return;
        
        foreach (StreamEntry streamEntry in streamEntries)
        {
            foreach (NameValueEntry entry in streamEntry.Values)
            {
                if (entry.Value.IsNullOrEmpty) continue;
        
                KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
                if (kumaResponse == null) continue;
                
                if (kumaResponse.ChannelId == null) return;
        
                // Send an image with caption if one is attached
                if (kumaResponse.Image != null)
                {
                    string caption = $"{kumaResponse.Image.Description}";
                    if (kumaResponse.Image.Referrer != "" && kumaResponse.Image.Source != "") caption += $"\n\n[{kumaResponse.Image.Referrer}]({kumaResponse.Image.Source})";

                    // Try sending the image first, then a link if that fails. Usually fails when the image is too large
                    try
                    {
                        await _telegramClient.SendPhotoAsync(
                            chatId: kumaResponse.ChannelId, 
                            photo: InputFile.FromUri(kumaResponse.Image.Url), 
                            caption: caption, 
                            parseMode: ParseMode.Markdown);
                    }
                    catch
                    {
                        await _telegramClient.SendTextMessageAsync(
                            chatId: kumaResponse.ChannelId, 
                            text: $"Image was too large for telegram.\n\n[{kumaResponse.Image.Referrer}]({kumaResponse.Image.Source})\n\n{kumaResponse.Image.Description}",
                            parseMode: ParseMode.Markdown);
                    }
            
                }
                // Send a standard message
                else if (!string.IsNullOrEmpty(kumaResponse.Message))
                {
                    await _telegramClient.SendTextMessageAsync(chatId: kumaResponse.ChannelId, text: kumaResponse.Message);
                }
            }
        }
    }
}