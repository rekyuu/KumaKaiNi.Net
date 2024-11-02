using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpToken;

namespace KumaKaiNi.Core.Commands;

public static class TextGenerationCommands
{
    private const string? RuleErrorResponse = "Usage: !gptrule [add <rule> | update <rule_id> <rule> | del <rule_id>]";

    private static GptEncoding? _aiModelEncoding;
    
    [Command(["ai", "gpt"])]
    public static async Task<KumaResponse?> GptResponseAsync(KumaRequest kumaRequest)
    {
        try
        {
            string lockKey = $"ai:{Enum.GetName(kumaRequest.SourceSystem)}:{kumaRequest.ChannelId}";
            await using LockHandle @lock = new(lockKey);

            bool lockObtained = await @lock.TryAcquireAsync(0);
            if (!lockObtained)
            {
                Log.Verbose("Lock already obtained for {LockKey}, skipping", lockKey);
                return null;
            }

            // Try OpenAI if available, or Ollama otherwise. Return a random GPT2 response on either failing
            if (!string.IsNullOrEmpty(KumaRuntimeConfig.OpenAiApiKey)) return await GetOpenAiResponse(kumaRequest);
            return await CannedGpt2ResponseAsync();
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, 
                "An exception occurred while processing AI generation request, returning a canned GPT2 response instead");
            return await CannedGpt2ResponseAsync();
        }
    }
    
    [Command("gpt2")]
    public static async Task<KumaResponse> CannedGpt2ResponseAsync()
    {
        await using KumaKaiNiDbContext db = new();
        GptResponse? result = await db.GptResponses
            .FirstOrDefaultAsync(x => x.Returned == false);

        if (result == null) return new KumaResponse("I have nothing more to say...");

        result.Returned = true;
        await db.SaveChangesAsync();

        return new KumaResponse(result.Message);
    }

    [Command("gdq")]
    public static async Task<KumaResponse?> GdqAsync()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "https://taskinoz.com/gdq/api/");
        HttpResponseMessage response = await Rest.SendAsync(request);
        
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync();

        return !string.IsNullOrEmpty(content) ? new KumaResponse(content) : null;
    }

    [Command("gptmodel", UserAuthority.Administrator)]
    public static async Task<KumaResponse?> UpdateGptModel(KumaRequest kumaRequest)
    {
        AdminConfig config = await KumaRuntimeConfig.GetConfigFromDatabaseAsync();

        if (kumaRequest.CommandArgs.Length == 0)
        {
            return new KumaResponse($"OpenAI GPT model is currently set to `{config.OpenAiModel}`.");
        }

        await using KumaKaiNiDbContext db = new();
        string model = kumaRequest.CommandArgs.First();
        config.OpenAiModel = model;

        await db.SaveChangesAsync();

        return new KumaResponse($"OpenAI GPT model changed to `{model}`.");
    }

    [Command("gpttokenlimit", UserAuthority.Administrator)]
    public static async Task<KumaResponse?> UpdateGptTokenLimit(KumaRequest kumaRequest)
    {
        AdminConfig config = await KumaRuntimeConfig.GetConfigFromDatabaseAsync();

        if (kumaRequest.CommandArgs.Length == 0)
        {
            return new KumaResponse($"OpenAI token limit is currently set to `{config.OpenAiTokenLimit}`.");
        }

        string tokenLimit = kumaRequest.CommandArgs.First();

        await using KumaKaiNiDbContext db = new();

        bool tokenLimitParsed = long.TryParse(tokenLimit, out long tokenLimitResult);
        if (!tokenLimitParsed)
        {
            return new KumaResponse($"Unable to set `{tokenLimit}` as the OpenAI token limit.");
        }

        config.OpenAiTokenLimit = tokenLimitResult;
        await db.SaveChangesAsync();

        return new KumaResponse($"OpenAI token limit changed to `{tokenLimit}`.");
    }

    [Command("gptprompt", UserAuthority.Administrator)]
    public static async Task<KumaResponse?> GetGptPrompt(KumaRequest kumaRequest)
    {
        AdminConfig config = await KumaRuntimeConfig.GetConfigFromDatabaseAsync();

        return new KumaResponse(config.AiInitialPrompt);
    }

    [Command("gptrule", UserAuthority.Administrator)]
    public static async Task<KumaResponse?> AddGptRule(KumaRequest kumaRequest)
    {
        await using KumaKaiNiDbContext db = new();

        switch (kumaRequest.CommandArgs.Length)
        {
            // Get list of rules
            case 0:
            {
                string[] rules = await db.AiPromptRules
                    .Select(x => $"[{x.RuleId}] {x.Rule}")
                    .ToArrayAsync();

                return new KumaResponse(string.Join("\n", rules));
            }
            // Get a specific rule by ID
            case 1:
            {
                bool parsed = long.TryParse(kumaRequest.CommandArgs[0], out long ruleId);
                if (!parsed) return new KumaResponse(RuleErrorResponse);

                AiPromptRule? ruleById = await db.AiPromptRules.FirstOrDefaultAsync(x => x.RuleId == ruleId);
                return ruleById != null ? new KumaResponse($"[{ruleById.RuleId}] {ruleById.Rule}") : null;
            }
            case > 1:
            {
                string? responseText;
                switch (kumaRequest.CommandArgs[0])
                {
                    // Add a new rule
                    case "add":
                    case "new":
                        string newRuleText = string.Join(" ", kumaRequest.CommandArgs[1..]);

                        AiPromptRule newRule = new(newRuleText);
                        await db.AddAsync(newRule);

                        responseText = "Rule added.";
                        break;
                    // Modify a rule
                    case "update":
                    case "modify":
                        bool updateIdParsed = long.TryParse(kumaRequest.CommandArgs[1], out long ruleIdToUpdate);
                        if (!updateIdParsed) return new KumaResponse($"Unable to parse rule ID {ruleIdToUpdate}");

                        AiPromptRule? ruleToUpdate = await db.AiPromptRules
                            .FirstOrDefaultAsync(x => x.RuleId == ruleIdToUpdate);

                        if (ruleToUpdate == null) return new KumaResponse($"Rule ID {ruleIdToUpdate} does not exist.");

                        ruleToUpdate.Rule = string.Join(" ", kumaRequest.CommandArgs[2..]);

                        responseText = "Rule added.";
                        break;
                    // Delete an existing quote by ID
                    case "del":
                    case "delete":
                    case "rem":
                    case "remove":
                        bool removeIdParsed = long.TryParse(kumaRequest.CommandArgs[1], out long ruleIdToRemove);
                        if (!removeIdParsed) return new KumaResponse($"Unable to parse rule ID {ruleIdToRemove}");

                        AiPromptRule? ruleToRemove = await db.AiPromptRules
                            .FirstOrDefaultAsync(x => x.RuleId == ruleIdToRemove);

                        if (ruleToRemove == null) return null;

                        db.AiPromptRules.Remove(ruleToRemove);

                        responseText = "Rule removed.";
                        break;
                    default:
                        return new KumaResponse(RuleErrorResponse);
                }

                await db.SaveChangesAsync();
                return new KumaResponse(responseText);
            }
            default:
            {
                return new KumaResponse(RuleErrorResponse);
            }
        }
    }

    private static async Task<KumaResponse?> GetOpenAiResponse(KumaRequest kumaRequest)
    {
        List<OpenAiChatMessage> messages = await GetChatHistoryMessagesAsync(kumaRequest);
        if (messages.Last().Role == "assistant") return null;

        AdminConfig config = await KumaRuntimeConfig.GetConfigFromDatabaseAsync();

        // Keep the outgoing tokens under the max limit
        long tokens = GetTokenCount(messages, config.OpenAiModel);
        while (tokens > config.OpenAiTokenLimit && messages.Count >= 1)
        {
            messages.RemoveAt(1);
            tokens = GetTokenCount(messages, config.OpenAiModel);
        }

        // https://platform.openai.com/docs/api-reference/chat/create
        OpenAiChatRequest openAiChatRequest = new(messages, config.OpenAiModel);
        Log.Verbose("Sending OpenAI request containing {MessageCount} messages for {TokenCount} tokens",
            openAiChatRequest.Messages.Count, 
            tokens);
        
        JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };
        
        HttpRequestMessage httpRequest = new(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(openAiChatRequest, options: jsonOptions)
        };
        
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", KumaRuntimeConfig.OpenAiApiKey);

        Stopwatch stopwatch = new();
        stopwatch.Start();
        HttpResponseMessage httpResponse = await Rest.SendAsync(httpRequest);
        string content = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            Log.Error("OpenAI request did not return success: {StatusCode} {Content}", 
                httpResponse.StatusCode, content);
            return null;
        }
        stopwatch.Stop();
        
        OpenAiChatResponse? openAiChatResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(content);
        Log.Verbose("OpenAI request took {Elapsed} to complete and used {Tokens} tokens", 
            stopwatch.Elapsed,
            openAiChatResponse?.Usage?.TotalTokens);

        string? message = openAiChatResponse?.Choices?.First().Message?.Content;
        if (!string.IsNullOrEmpty(message)) return new KumaResponse(message);
        
        Log.Error("OpenAI returned an empty response: {Content}", content);
        return null;
    }

    private static async Task<List<OpenAiChatMessage>> GetChatHistoryMessagesAsync(KumaRequest kumaRequest)
    {
        List<OpenAiChatMessage> messages = [];
        
        // Start with the system message
        OpenAiChatMessage initialSystemMessage = new(await GetInitialSystemMessage(), "system");
        messages.Add(initialSystemMessage);
        
        await using KumaKaiNiDbContext db = new();
        ChatLog[] logs = await db.ChatLogs
            .Where(x => x.SourceSystem == kumaRequest.SourceSystem)
            .Where(x => x.ChannelId == kumaRequest.ChannelId)
            .Where(x => x.Timestamp >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(x => x.Timestamp)
            .ToArrayAsync();

        bool commandResponse = false;
        foreach (ChatLog log in logs)
        {
            // Skip if Kuma is (probably) responding to a command
            if (log.Username == "KumaKaiNi" && commandResponse)
            {
                commandResponse = false;
                continue;
            }

            string chatLogMessageContent = log.Message;
            if (log.Message.StartsWith('!'))
            {
                // Skip if the message was a non text generation command request
                if (!log.Message.StartsWith("!gpt") && !log.Message.StartsWith("!ai"))
                {
                    commandResponse = true;
                    continue;
                }

                chatLogMessageContent = log.Message
                    .Replace("!gpt", "")
                    .Replace("!ai", "")
                    .Trim();
                if (string.IsNullOrEmpty(chatLogMessageContent)) continue;
            }

            OpenAiChatMessage chatLogMessage;
            if (log.Username == "KumaKaiNi")
            {
                chatLogMessage = new OpenAiChatMessage(chatLogMessageContent, "assistant");
            }
            else
            {
                chatLogMessage = new OpenAiChatMessage($"{log.Username}: {chatLogMessageContent}", "user");
            }
        
            messages.Add(chatLogMessage);
        }

        return messages;
    }

    private static long GetTokenCount(IEnumerable<OpenAiChatMessage> messages, string modelName = "gpt-4")
    {
        _aiModelEncoding ??= GptEncoding.GetEncodingForModel(modelName);
        long tokens = 0;

        foreach (OpenAiChatMessage message in messages)
        {
            tokens += _aiModelEncoding.CountTokens(message.Content);
        }

        return tokens;
    }

    private static async Task<string> GetInitialSystemMessage()
    {
        await using KumaKaiNiDbContext db = new();
        AdminConfig config = await KumaRuntimeConfig.GetConfigFromDatabaseAsync();

        string prompt = config.AiInitialPrompt;
        string[] rules = await db.AiPromptRules.Select(x => $"- {x.Rule}").ToArrayAsync();

        return $"{prompt}\n\n{string.Join("\n", rules)}";
    }
}
