using System.Text.RegularExpressions;
using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KumaKaiNi.Core.Commands;

public static class TextGenerationCommands
{
    private const int MarkovChainLength = 3;

    [Command("gdq")]
    public static async Task<KumaResponse?> GdqAsync()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "https://taskinoz.com/gdq/api/");
        HttpResponseMessage response = await Rest.SendAsync(request);

        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync();

        return !string.IsNullOrEmpty(content) ? new KumaResponse(content) : null;
    }

    [Command("markov")]
    public static async Task<KumaResponse?> MarkovAsync(KumaRequest kumaRequest)
    {
        if (kumaRequest is { UserAuthority: UserAuthority.Administrator, CommandArgs: ["train"] })
        {
            await TrainMarkov(kumaRequest.SourceSystem, kumaRequest.ChannelId);
            return new KumaResponse("done training for markov chain generation, kuma");
        }

        string content = await GetMarkovText(kumaRequest.SourceSystem, kumaRequest.ChannelId);
        return !string.IsNullOrEmpty(content) ? new KumaResponse(content) : null;
    }

    public static async Task AddMessageToMarkovModel(SourceSystem sourceSystem, string? channelId, string message)
    {
        await using KumaKaiNiDbContext db = new();

        string[] sentences = message.Split(". ");

        foreach (string sentence in sentences)
        {
            string[] words = CleanMessage(sentence);

            for (int i = 0; i < words.Length - MarkovChainLength + 1; i++)
            {
                bool isStart = i == 0;
                string previousWords = string.Join(' ', words[i..(i + MarkovChainLength - 1)]);

                string nextWord = "";
                int nextWordIndex = i + MarkovChainLength - 1;
                if (nextWordIndex < words.Length) nextWord = words[nextWordIndex];

                Markov? entry = await db.Markov
                    .Where(x => x.SourceSystem == sourceSystem && x.ChannelId == channelId &&
                                x.PreviousWords == previousWords && x.NextWord == nextWord)
                    .FirstOrDefaultAsync();

                if (entry != null)
                {
                    entry.Count += 1;
                    if (isStart && !entry.CanStart) entry.CanStart = true;
                }
                else
                {
                    entry = new Markov(sourceSystem, channelId, previousWords, nextWord, 1, isStart);
                    await db.Markov.AddAsync(entry);
                }

                await db.SaveChangesAsync();
            }
        }
    }

    private static string[] CleanMessage(string message)
    {
        List<string> cleanedWords = [];

        foreach (string word in message.Split(' '))
        {
            if (word == "I")
            {
                cleanedWords.Add(word);
                continue;
            }

            string cleanedWord = word.ToLowerInvariant();

            cleanedWord = Regex.Replace(cleanedWord, @"<(.*?):", string.Empty);
            cleanedWord = Regex.Replace(cleanedWord, @":(.*?)>", string.Empty);
            cleanedWord = Regex.Replace(cleanedWord, @"<@(.*?)>", string.Empty);
            cleanedWord = Regex.Replace(cleanedWord, @"@", string.Empty);

            if (!string.IsNullOrEmpty(cleanedWord.Trim())) cleanedWords.Add(cleanedWord);
        }

        return cleanedWords.ToArray();
    }

    private static async Task TrainMarkov(SourceSystem sourceSystem, string? channelId)
    {
        await using KumaKaiNiDbContext db = new();

        Log.Information("Clearing previously trained data for {SourceSystem}#{ChannelId}", sourceSystem, channelId);

        await db.Markov
            .Where(x => x.SourceSystem == sourceSystem && x.ChannelId == channelId)
            .ExecuteDeleteAsync();

        string[] messages = db.ChatLogs
            .Where(x => x.SourceSystem == sourceSystem && x.ChannelId == channelId &&
                        x.Username != "KumaKaiNi")
            .Select(x => x.Message)
            .ToArray();

        Log.Information("Training {SourceSystem}#{ChannelId} ({Count} messages)", sourceSystem, channelId, messages.Length);

        int i = 1;
        foreach (string message in messages)
        {
            if (message.StartsWith('!')) continue;
            if (message.StartsWith('/')) continue;

            await AddMessageToMarkovModel(sourceSystem, channelId, message);

            Log.Debug("Trained {Iteration}/{Count} messages", i, messages.Length);
            i++;
        }

        Log.Information("Finished training {SourceSystem}#{ChannelId}", sourceSystem, channelId);
    }

    private static async Task<string> GetMarkovText(SourceSystem sourceSystem, string? channelId)
    {
        await using KumaKaiNiDbContext db = new();

        string? startingWords = await db.Markov
            .Where(x => x.SourceSystem == sourceSystem && x.ChannelId == channelId && x.CanStart == true)
            .OrderBy(_ => Guid.NewGuid())
            .Select(x => x.PreviousWords)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(startingWords)) return "";

        List<string> result = startingWords.Split(' ').ToList();
        while (true)
        {
            string previousWords = string.Join(' ', result.TakeLast(MarkovChainLength - 1));

            Markov[] markovResults = db.Markov
                .Where(x => x.SourceSystem == sourceSystem && x.ChannelId == channelId &&
                            x.PreviousWords == previousWords)
                .ToArray();

            if (markovResults.Length == 0) break;

            WeightedPool<Markov> markovPool = new();
            foreach (Markov markov in markovResults) markovPool.AddElement(markov, markov.Count);
            markovPool.UpdateWeights();

            Markov nextMarkov = markovPool.GetRandomElement();
            if (nextMarkov.NextWord == "") break;

            result.Add(nextMarkov.NextWord);

            if (nextMarkov.NextWord.EndsWith('.')) break;
        }

        return string.Join(' ', result);
    }
}
