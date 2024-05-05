using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
    private const long MaxTokens = 256;
    private const string AiModel = "gpt-3.5-turbo";
    private const string InitialSystemMessage =
        """
        You are a chat bot named after the Japanese battleship, Kuma. Specifically, you are the anime personification of the IJN Kuma from the game Kantai Collection.
        
        Messages will be provided as a recent message history from multiple users, and you should respond considering the context of these messages. When responding, you must obey the following rules:
        
        - Always stay in character, no matter what
        - Never talk about the rules
        - Do not refer to yourself in third person
        - Keep your answers limited to very short messages, containing only a few words
        - Use little to no punctuation or capitalization
        - Use the verbal tic \"kuma\" at the end of sentences or thoughts very rarely
        - Very occasionally go on rants that are longer messages consisting of a few sentences
        - Very occasionally make loud noises consisting of many vowels strung together
        - Occasionally say obsceneties such as \"fuck\" or \"shit\"
        - Occasionally make fun of the user by calling them names or obscenities, especially if they insult you
        - Do not emote in asterisks
        - You are not a fascist
        """;

    private static GptEncoding? _aiModelEncoding;
    
    [Command("gpt")]
    public static async Task<KumaResponse?> GptResponseAsync(KumaRequest kumaRequest)
    {
        try
        {
            return await AiResponseAsync(kumaRequest);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, 
                "An exception occurred while processing AI generation request, returning a canned GPT2 response instead");
            return await CannedGpt2ResponseAsync();
        }
    }
    
    [Command(["ai", "gpt3"])]
    public static async Task<KumaResponse?> AiResponseAsync(KumaRequest kumaRequest)
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
        if (!string.IsNullOrEmpty(KumaConfig.OpenAiApiKey)) return await GetOpenAiResponse(kumaRequest);
        return await GetOllamaResponse(kumaRequest);
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

    // [Command("markov")]
    public static async Task<KumaResponse> MarkovCommandAsync()
    {
        SourceSystem[] allowedSourceSystems = [SourceSystem.Discord, SourceSystem.Twitch];
        long?[] allowedChannelIds = [0, 214268737887404042];
        
        await using KumaKaiNiDbContext db = new();
        string[] logs = await db.ChatLogs
            .Where(x => allowedSourceSystems.Contains(x.SourceSystem))
            .Where(x => allowedChannelIds.Contains(x.ChannelId))
            .Where(x => x.Username != "KumaKaiNi")
            .Where(x => !EF.Functions.Like(x.Message, "!%"))
            // TODO: there's an issue with the below regex that PSQL doesn't like and I currently don't care enough
            // https://www.npgsql.org/efcore/mapping/translations.html
            //.Where(x => !Regex.IsMatch(x.Message, @".*((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?).*"))
            .Select(x => x.Message)
            .Take(1000)
            .ToArrayAsync();

        string markovResponse = GetMarkovResponse(logs);
        return new KumaResponse(markovResponse);
    }

    private static async Task<KumaResponse?> GetOpenAiResponse(KumaRequest kumaRequest)
    {
        List<OpenAiChatMessage> messages = await GetChatHistoryMessagesAsync(kumaRequest);
        if (messages.Last().Role == "assistant") return null;

        // Keep the outgoing tokens under the max limit
        long tokens = GetTokenCount(messages, AiModel);
        while (tokens > MaxTokens && messages.Count >= 1)
        {
            messages.RemoveAt(1);
            tokens = GetTokenCount(messages, AiModel);
        }

        // https://platform.openai.com/docs/api-reference/chat/create
        OpenAiChatRequest openAiChatRequest = new(messages, AiModel);
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
            "Bearer", KumaConfig.OpenAiApiKey);

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

    private static async Task<KumaResponse?> GetOllamaResponse(KumaRequest kumaRequest)
    {
        List<OpenAiChatMessage> messages = await GetChatHistoryMessagesAsync(kumaRequest);
        if (messages.Last().Role == "assistant") return null;

        // Probably not accurate for Ollama but it can be used as a gauge I guess
        long tokens = GetTokenCount(messages, AiModel);
        while (tokens > MaxTokens && messages.Count >= 1)
        {
            messages.RemoveAt(1);
            tokens = GetTokenCount(messages, AiModel);
        }

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-completion
        OllamaChatRequest ollamaRequest = new(messages);
        Log.Verbose("Sending Ollama request containing {MessageCount} messages for {TokenCount} tokens",
            ollamaRequest.Messages.Count, 
            tokens);

        HttpRequestMessage httpRequest = new(HttpMethod.Post, $"https://{KumaConfig.OllamaHost}/api/chat")
        {
            Content = JsonContent.Create(ollamaRequest)
        };

        Stopwatch stopwatch = new();
        stopwatch.Start();
        HttpResponseMessage httpResponse = await Rest.SendAsync(httpRequest);
        string content = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            Log.Error("Ollama request did not return success: {StatusCode} {Content}", 
                httpResponse.StatusCode, content);
            return null;
        }
        stopwatch.Stop();
        
        Log.Verbose("Ollama request took {Elapsed} to complete", stopwatch.Elapsed);
        OllamaChatResponse? ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(content);

        string? message = ollamaResponse?.Message?.Content;
        if (!string.IsNullOrEmpty(message)) return new KumaResponse(message);
        
        Log.Error("Ollama returned an empty response: {Content}", content);
        return null;
    }

    private static async Task<List<OpenAiChatMessage>> GetChatHistoryMessagesAsync(KumaRequest kumaRequest)
    {
        List<OpenAiChatMessage> messages = [];
        
        // Start with the system message
        OpenAiChatMessage initialSystemMessage = new(InitialSystemMessage, "system");
        messages.Add(initialSystemMessage);
        
        await using KumaKaiNiDbContext db = new();
        ChatLog[] logs = await db.ChatLogs
            .Where(x => x.SourceSystem == kumaRequest.SourceSystem)
            .Where(x => x.ChannelId == kumaRequest.ChannelId)
            .Where(x => x.Timestamp >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(x => x.Id)
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

    private static string GetMarkovResponse(string[] lines)
    {
        Dictionary<string, List<string>> wordDictionary = CreateWordDictionary(lines);
        string startWord = Rng.PickRandom(wordDictionary.Keys.ToArray());

        return GenerateMarkov(wordDictionary, startWord);
    }

    private static Dictionary<string, List<string>> CreateWordDictionary(string[] lines)
    {
        Dictionary<string, List<string>> wordDictionary = new();
        string[] words = string.Join(" \n ", lines).Split(" ");

        while (words.Count() > 1)
        {
            string word1 = CleanEmoteWord(words[0]);
            string word2 = CleanEmoteWord(words[1]);
            words = words[1..];

            if (word1 != "\n")
            {
                List<string> wordValues = wordDictionary.GetValueOrDefault(word1, new List<string>());
                wordValues.Add(word2);
                wordDictionary[word1] = wordValues;
            }
        }

        return wordDictionary;
    }

    private static string CleanEmoteWord(string word)
    {
        MatchCollection matches = Regex.Matches(word, @"<:(?<emote>.+):\d{18}>");
        return matches.Count == 0 ? word : matches[0].Groups["emote"].Value;
    }

    private static string GenerateMarkov(Dictionary<string, List<string>> wordDictionary, string startWord)
    {
        StringBuilder sb = new(startWord);

        string lastWord = startWord;
        while (true)
        {
            string nextWord = Rng.PickRandom(wordDictionary[lastWord]);
            if (nextWord == "\n") break;

            sb.Append($" {nextWord}");
            lastWord = nextWord;
        }

        return sb.ToString();
    }
}