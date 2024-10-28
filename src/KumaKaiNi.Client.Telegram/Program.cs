using System.Text.Json;
using KumaKaiNi.Core;
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

namespace KumaKaiNi.Client.Telegram;

internal static class Program
{
    private static readonly string[] SupportedVideoFileTypes = [ "mp4" ];

    private static RedisStreamConsumer? _streamConsumer;
    private static ITelegramBotClient? _telegramClient;
    private static CancellationTokenSource? _cts;
    private static string? _botUsername;
    
    private static async Task Main()
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(KumaConfig.GetLogLevel())
                .WriteTo.Console()
                .CreateLogger();
            
            Log.Information("Starting {ApplicationName} {ApplicationVersion} on {MachineName}", 
                KumaConfig.ApplicationName, 
                KumaConfig.ApplicationVersion, 
                Environment.MachineName);

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
                Redis.GetStreamNameForSourceSystem(SourceSystem.Telegram),
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
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
            await Redis.SendDeploymentNotificationToAdmin();
            
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
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task HandleTelegramUpdateAsync(ITelegramBotClient telegramClient, Update update, CancellationToken ct)
    {
        try
        {
            // Skip update if no message exists or messages from self
            if (update.Message is not { } message) return;
            if (message.From?.Id == telegramClient.BotId) return;

            // Determine user authority and if the chat is a private chat
            bool isAdmin = message.From?.Id == KumaConfig.TelegramAdminId;
            bool isPrivate = message.Chat.Id == message.From?.Id;
            UserAuthority authority = isAdmin ? UserAuthority.Administrator : UserAuthority.User;

            // Skip if it's a private chat that isn't from the administrator
            if (!isAdmin && isPrivate) return;

            // Determine if the chat is allowed
            await using KumaKaiNiDbContext db = new();
            TelegramAllowList? allowList = await db.TelegramAllowList
                .Where(x => x.ChannelId == message.Chat.Id.ToString())
                .FirstOrDefaultAsync(ct);

            if (allowList == null)
            {
                allowList = new TelegramAllowList(message.Chat.Id.ToString());

                await db.TelegramAllowList.AddAsync(allowList, ct);
                await db.SaveChangesAsync(ct);
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
                        await Logging.LogExceptionToDatabaseAsync(ex, "[KumaKaiNi.Client.Telegram] Exception was thrown while trying to leave chat {ChatId}", message.Chat.Id);
                    }
                    
                    return;
                }

                allowList.Warnings++;
                await db.SaveChangesAsync(ct);
                
                return;
            }
            
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
                message.MessageId.ToString(),
                authority,
                message.Chat.Id.ToString(),
                isPrivate,
                true);

            await Redis.AddRequestToStreamAsync(kumaRequest);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "Failed to process update: {UpdateType}", update.Type);
        }
    }

    private static async Task HandleTelegramPollingErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
    {
        await Logging.LogExceptionToDatabaseAsync(ex, "An exception was thrown while polling Telegram");
    }

    // ReSharper disable once UnusedMember.Local
    private static void OnKumaProcessing(long? channelId)
    {
        if (_telegramClient == null) return;
        if (channelId == null) return;
        
        _ = _telegramClient.SendChatActionAsync(chatId: channelId, chatAction: ChatAction.Typing);
    }

    private static async void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (_telegramClient == null) return;
        if (entry.Value.IsNullOrEmpty) return;
        
        KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (kumaResponse?.ChannelId == null) return;
        
        // Send an image with caption if one is attached
        if (kumaResponse.Media != null)
        {
            string caption = $"{kumaResponse.Media.Description}";
            if (kumaResponse.Media.Referrer != "" && kumaResponse.Media.Source != "") caption += $"\n\n[{kumaResponse.Media.Referrer}]({kumaResponse.Media.Source})";

            // Try sending the media first, then a link if that fails. Usually fails when the image is too large
            try
            {
                if (SupportedVideoFileTypes.Any(x => kumaResponse.Media.Url.EndsWith(x)))
                {
                    await _telegramClient.SendVideoAsync(
                        chatId: kumaResponse.ChannelId,
                        video: InputFile.FromUri(kumaResponse.Media.Url),
                        caption: caption,
                        parseMode: ParseMode.Markdown);
                }
                else
                {
                    await _telegramClient.SendPhotoAsync(
                        chatId: kumaResponse.ChannelId,
                        photo: InputFile.FromUri(kumaResponse.Media.Url),
                        caption: caption,
                        parseMode: ParseMode.Markdown);
                }
            }
            catch
            {
                await _telegramClient.SendPhotoAsync(
                    chatId: kumaResponse.ChannelId,
                    photo: InputFile.FromUri(kumaResponse.Media.Preview),
                    caption: $"Media was too large for Telegram.\n\n{caption}",
                    parseMode: ParseMode.Markdown);
            }
            
        }
        // Send a standard message
        else if (!string.IsNullOrEmpty(kumaResponse.Message))
        {
            await _telegramClient.SendTextMessageAsync(
                chatId: kumaResponse.ChannelId, 
                text: kumaResponse.Message,
                disableWebPagePreview: true,
                parseMode: ParseMode.Markdown);
        }
    }
}