using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KumaKaiNi.Core.Commands;

public static class DanbooruCommands
{
    [Command("dan", nsfw: true)]
    public static async Task<KumaResponse> GetDanbooruAsync(KumaRequest kumaRequest)
    {
        ResponseMedia? media = await GetDanbooruImageAsync(kumaRequest.CommandArgs, kumaRequest.SourceSystem, kumaRequest.ChannelId);
        return media?.Url != null ? new KumaResponse { Media = media } : new KumaResponse("Nothing found!");
    }

    [Command(["safe", "sfw"])]
    public static async Task<KumaResponse> GetSafeDanbooruAsync(KumaRequest kumaRequest)
    {
        string tagToUse = "rating:g";
        if (Rng.OneTo(4)) tagToUse = "rating:s";

        string[] baseTags = [tagToUse];
        string[] requestTags = baseTags.Concat(kumaRequest.CommandArgs).ToArray();
        ResponseMedia? media = await GetDanbooruImageAsync(requestTags, kumaRequest.SourceSystem, kumaRequest.ChannelId);

        return media?.Url != null ? new KumaResponse { Media = media } : new KumaResponse("Nothing found!");
    }

    [Command("lewd", nsfw: true)]
    public static async Task<KumaResponse> GetLewdDanbooruAsync(KumaRequest kumaRequest)
    {
        string[] baseTags = ["rating:q"];
        string[] requestTags = baseTags.Concat(kumaRequest.CommandArgs).ToArray();
        ResponseMedia? media = await GetDanbooruImageAsync(requestTags, kumaRequest.SourceSystem, kumaRequest.ChannelId);

        return media?.Url != null ? new KumaResponse { Media = media } : new KumaResponse("Nothing found!");
    }

    [Command("xxx", nsfw: true)]
    public static async Task<KumaResponse> GetExplicitDanbooruAsync(KumaRequest kumaRequest)
    {
        string[] baseTags = ["rating:e"];
        string[] requestTags = baseTags.Concat(kumaRequest.CommandArgs).ToArray();
        ResponseMedia? media = await GetDanbooruImageAsync(requestTags, kumaRequest.SourceSystem, kumaRequest.ChannelId);

        return media?.Url != null ? new KumaResponse { Media = media } : new KumaResponse("Nothing found!");
    }

    [Command("nsfw", nsfw: true)]
    public static async Task<KumaResponse> GetNsfwDanbooruAsync(KumaRequest kumaRequest)
    {
        string[] baseTags = [Rng.PickRandom(["rating:q", "rating:e"])];
        string[] requestTags = baseTags.Concat(kumaRequest.CommandArgs).ToArray();
        ResponseMedia? media = await GetDanbooruImageAsync(requestTags, kumaRequest.SourceSystem, kumaRequest.ChannelId);

        return media?.Url != null ? new KumaResponse { Media = media } : new KumaResponse("Nothing found!");
    }
    
    [Command("danban", UserAuthority.Administrator)]
    public static async Task<KumaResponse> BlockTagsAsync(KumaRequest kumaRequest)
    {
        await using KumaKaiNiDbContext db = new();
        string[] blockedTags = await db.DanbooruBlockList
            .Select(x => x.Tag)
            .ToArrayAsync();

        int inserted = 0;
        foreach (string tag in kumaRequest.CommandArgs)
        {
            if (blockedTags.Contains(tag)) continue;

            DanbooruBlockList newTag = new(tag);
            await db.DanbooruBlockList.AddAsync(newTag);
            inserted++;
        }

        if (inserted == 0) return new KumaResponse("Nothing to add.");
        
        await db.SaveChangesAsync();
        return new KumaResponse($"{inserted} tags added.");
    }

    [Command("danunban", UserAuthority.Administrator)]
    public static async Task<KumaResponse> AllowTagsAsync(KumaRequest kumaRequest)
    {
        await using KumaKaiNiDbContext db = new();

        int deleted = 0;
        foreach (string tag in kumaRequest.CommandArgs)
        {
            DanbooruBlockList? blockedTag = await db.DanbooruBlockList
                .FirstOrDefaultAsync(x => x.Tag == tag);
            
            if (blockedTag == null) continue;

            db.DanbooruBlockList.Remove(blockedTag);
            deleted++;
        }

        if (deleted == 0) return new KumaResponse("Nothing to remove.");
        
        await db.SaveChangesAsync();
        return new KumaResponse($"{deleted} tags removed.");
    }

    private static async Task<ResponseMedia?> GetDanbooruImageAsync(string[] tags, SourceSystem sourceSystem, string? channelId)
    {
        // Skip the request if it contains any banned tags
        await using KumaKaiNiDbContext db = new();
        string[] blockedTags = await db.DanbooruBlockList
            .Select(x => x.Tag)
            .ToArrayAsync();

        if (tags.Any(tag => blockedTags.Contains(tag))) return null;

        // Fetch recent returns from cache
        string cacheKeyPrefix = $"danbooru:{Enum.GetName(sourceSystem)!.ToLowerInvariant()}:{channelId}";
        string? cacheKey = null;
        string[] cachedResults = Cache.GetCachedKeys(cacheKeyPrefix);
        
        // Create the base HTTP request parameters
        int page = 1;
        const int limit = 50;
        
        UriBuilder uriBuilder = new("https://danbooru.donmai.us/posts.json");
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
        query.Add("limit", limit.ToString());
        if (tags.Length > 0) query.Add("tags", string.Join(" ", tags));

        byte[] authToken = Encoding.ASCII.GetBytes($"{KumaConfig.DanbooruUser}:{KumaConfig.DanbooruApiKey}");
        string encodedToken = Convert.ToBase64String(authToken);
        AuthenticationHeaderValue authHeader = new("Basic", encodedToken);
        
        // Iterate over Danbooru results until something has been found
        DanbooruResult? result = null;
        while (true)
        {
            // Fetch a page of results
            query.Add("page", page.ToString());
            uriBuilder.Query = query.ToString();

            HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Authorization = authHeader;
            
            HttpResponseMessage response = await Rest.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Response from Danbooru did not indicate success: {Response}", response);
                return null;
            }
            
            string content = await response.Content.ReadAsStringAsync();
            List<DanbooruResult>? results = JsonSerializer.Deserialize<List<DanbooruResult>>(content);

            // No more results
            if (results == null || results.Count == 0) break;

            while (results.Count > 0)
            {
                // Grab something on the page at random
                DanbooruResult nextResult = Rng.PickRandom(results);
                results.Remove(nextResult);

                cacheKey = $"{cacheKeyPrefix}:{nextResult.Id}";

                if (cachedResults.Contains(cacheKey)) continue;
                if (string.IsNullOrEmpty(nextResult.FileUrl)) continue;
                if (nextResult.TagString == null) continue;
                if (nextResult.IsDeleted == true) continue;

                // If the image has any banned tags, don't return it
                string[] resultTags = nextResult.TagString.Split(" ");
                if (blockedTags.Any(tag => resultTags.Contains(tag))) continue;

                result = nextResult;
                
                break;
            }

            // Found something
            if (result != null) break;
            
            result = null;
            page++;
        }

        // Nothing found after iterating over all results
        if (result == null) return null;

        TextInfo ti = CultureInfo.InvariantCulture.TextInfo;

        // Specify the file URIs
        string fileUrl = ParseDanbooruFileUrl(result.FileUrl);
        string previewUrl = ParseDanbooruFileUrl(result.PreviewFileUrl);

        // Create the string for the characters
        string[]? characterTags = result.TagStringCharacter?.Split(" ");
        string characterString = "";
        if (characterTags?.Length > 0)
        {
            characterString = ti.ToTitleCase(characterTags[0].Split("(")[0].Replace("_", " "));

            switch (characterTags.Length)
            {
                case 2:
                {
                    string secondCharacter = ti.ToTitleCase(characterTags[1].Split("(")[0].Replace("_", " "));
                    characterString = $"{characterString} and {secondCharacter}";
                    break;
                }
                case > 2:
                    characterString = "Multiple";
                    break;
            }
        }

        // Create the string for the copyright
        string[]? copyrightTags = result.TagStringCopyright?.Split(" ");
        string copyrightString = "";
        if (copyrightTags?.Length > 0) copyrightString = ti.ToTitleCase(copyrightTags[0].Replace("_", " "));

        // Create the string for the artist
        string artistString = "";
        string[]? artist = result.TagStringArtist?.Split("_");
        if (artist != null) artistString = string.Join(" ", artist);

        // Create the string for the rating
        string ratingString = result.Rating switch
        {
            "g" => "General",
            "s" => "Sensitive",
            "q" => "Questionable",
            "e" => "Explicit",
            _ => "???"
        };

        // Create the full description string
        string descriptionString;
        if (characterString != "" && copyrightString != "") descriptionString = $"{characterString} - {copyrightString}";
        else if (copyrightString != "") descriptionString = $"Unknown - {copyrightString}";
        else descriptionString = "Original";

        if (!string.IsNullOrEmpty(artistString)) descriptionString += $"\nCreated by {artistString}";
        descriptionString += $"\nRating: {ratingString}";

        // Store the result in cache
        if (!string.IsNullOrEmpty(cacheKey))
        {
            await Cache.SetAsync(
                cacheKey,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                TimeSpan.FromDays(1));
        }

        return new ResponseMedia(
            fileUrl,
            previewUrl,
            $"https://danbooru.donmai.us/posts/{result.Id}", 
            descriptionString,
            "danbooru.donmai.us");

    }

    private static string ParseDanbooruFileUrl(string? fileUrl)
    {
        bool isValidUri = Uri.TryCreate(fileUrl, UriKind.RelativeOrAbsolute, out Uri? uriResult);
        bool isValidUriScheme = uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps;

        return (isValidUri && isValidUriScheme) ? fileUrl! : $"https://danbooru.donmai.us{fileUrl}";
    }
}