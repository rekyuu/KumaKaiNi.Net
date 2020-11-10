using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace KumaKaiNi.Core.Commands
{
    public static class Danbooru
    {
        [Command("dan")]
        public static Response GetDanbooru(Request request)
        {
            if (!request.ChannelIsNSFW) return new Response();

            ResponseImage image = GetDanbooruImage(request.CommandArgs, request.Protocol, request.ChannelId);

            if (image.URL != null) return new Response() { Image = image };
            else return new Response("Nothing found!");
        }

        [Command("safe")]
        [Command("sfw")]
        public static Response GetSafeDanbooru(Request request)
        {
            string[] baseTags = new string[] { "rating:s" };
            string[] requestTags = baseTags.Concat(request.CommandArgs).ToArray();
            ResponseImage image = GetDanbooruImage(request.CommandArgs, request.Protocol, request.ChannelId);

            if (image.URL != null) return new Response() { Image = image };
            else return new Response("Nothing found!");
        }

        [Command("lewd")]
        [Command("nsfw")]
        public static Response GetLewdDanbooru(Request request)
        {
            if (!request.ChannelIsNSFW) return new Response();

            string[] baseTags = new string[] { "-rating:s" };
            string[] requestTags = baseTags.Concat(request.CommandArgs).ToArray();
            ResponseImage image = GetDanbooruImage(request.CommandArgs, request.Protocol, request.ChannelId);

            if (image.URL != null) return new Response() { Image = image };
            else return new Response("Nothing found!");
        }

        private static ResponseImage GetDanbooruImage(string[] tags, RequestProtocol protocol, long channelID)
        {
            List<DanbooruBlocklist> blocklist = Database.GetMany<DanbooruBlocklist>();
            string[] blockedTags = new string[blocklist.Count];

            int i = 0;
            foreach (DanbooruBlocklist entry in blocklist)
            {
                blockedTags[i] = entry.Tag;
                i++;
            }

            foreach (string tag in tags)
            {
                if (blockedTags.Contains(tag)) return new ResponseImage();
            }

            string requestTags = string.Join("+", tags);
            string user = ConfigurationManager.AppSettings.Get("DanbooruUser");
            string pass = ConfigurationManager.AppSettings.Get("DanbooruAPIKey");
            byte[] authToken = Encoding.ASCII.GetBytes($"{user}:{pass}");
            int page = 1;
            int limit = 50;
            DanbooruResults result = null;

            WherePredicate whereChannel = new WherePredicate()
            {
                Source = "channel_id",
                Comparitor = "=",
                Target = channelID
            };

            WherePredicate whereProtocol = new WherePredicate()
            {
                Source = "protocol",
                Comparitor = "=",
                Target = protocol
            };

            string[] cache = new string[] { };
            if (channelID != 0)
            {
                WherePredicate whereExpires = new WherePredicate()
                {
                    Source = "expires",
                    Comparitor = ">",
                    Target = DateTime.UtcNow
                };

                List<DanbooruCache> danCache = Database.GetMany<DanbooruCache>(new WherePredicate[] { whereExpires, whereProtocol, whereChannel });

                i = 0;
                cache = new string[danCache.Count];
                foreach (DanbooruCache item in danCache)
                {
                    cache[i] = item.FileUrl;
                    i++;
                }
            }

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            while (true)
            {
                string requestURL = $"https://danbooru.donmai.us/posts.json?limit={limit}&page={page}&tags={requestTags}";
                HttpResponseMessage request = client.GetAsync(requestURL).Result;
                string content = request.Content.ReadAsStringAsync().Result;
                List<DanbooruResults> results = JsonConvert.DeserializeObject<List<DanbooruResults>>(content);

                if (results.Count == 0) break;

                result = RNG.PickRandom(results);
                results.Remove(result);

                bool badImage = false;
                while (results.Count > 0)
                {
                    if (badImage)
                    {
                        result = RNG.PickRandom(results);
                        results.Remove(result);
                        badImage = false;
                    }

                    if (cache.Contains(result.FileUrl))
                    {
                        badImage = true;
                        continue;
                    }

                    string[] resultTags = result.TagString.Split(" ");
                    foreach (string tag in blockedTags)
                    {
                        if (resultTags.Contains(tag))
                        {
                            badImage = true;
                            break;
                        }
                    }

                    if (!badImage) break;
                }

                if (!badImage) break;
                result = null;
                page++;
            }

            if (result != null)
            {
                TextInfo ti = new CultureInfo("en-US", false).TextInfo;

                bool isValidUri = Uri.TryCreate(result.FileUrl, UriKind.RelativeOrAbsolute, out Uri uriResult);
                bool isValidUriScheme = uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
                string fileUrl = (isValidUri && isValidUriScheme) ? result.FileUrl : $"http://danbooru.donmai.us{result.FileUrl}";

                string[] characterTags = result.TagStringCharacter.Split(" ");
                string characterString = "";
                if (characterTags.Length > 0)
                {
                    characterString = ti.ToTitleCase(characterTags[0].Split("(")[0].Replace("_", " "));

                    if (characterTags.Length == 2)
                    {
                        string secondCharacter = ti.ToTitleCase(characterTags[1].Split("(")[0].Replace("_", " "));
                        characterString = $"{characterString} and {secondCharacter}";
                    }
                    else if (characterTags.Length > 2) characterString = "Multiple";
                }

                string[] copyrightTags = result.TagStringCopyright.Split(" ");
                string copyrightString = "";
                if (copyrightTags.Length > 0) copyrightString = ti.ToTitleCase(copyrightTags[0].Replace("_", " "));

                string[] artist = result.TagStringArtist.Split("_");
                string artistString = string.Join(" ", artist);

                string descriptionString = "";
                if (characterString != "" && copyrightString != "") descriptionString = $"{characterString} - {copyrightString}";
                else if (copyrightString != "") descriptionString = $"Unknown - {copyrightString}";
                else descriptionString = $"Original";

                descriptionString += $"\nDrawn by {artistString}";

                if (channelID != 0)
                {
                    WherePredicate whereFileUrl = new WherePredicate
                    {
                        Source = "file_url",
                        Comparitor = "=",
                        Target = result.FileUrl
                    };

                    List<DanbooruCache> cachedItems = Database.GetMany<DanbooruCache>(new WherePredicate[] { whereFileUrl, whereProtocol, whereChannel });
                    if (cachedItems.Count > 1) throw new Exception("Multiple entries for file in Danbooru cache.");
                    else if (cachedItems.Count == 1)
                    {
                        cachedItems[0].Expires = DateTime.UtcNow.AddDays(1);
                        cachedItems[0].Update();
                    }
                    else
                    {
                        DanbooruCache cacheItem = new DanbooruCache()
                        {
                            FileUrl = result.FileUrl,
                            Expires = DateTime.UtcNow.AddDays(1),
                            Protocol = protocol,
                            ChannelId = channelID
                        };

                        cacheItem.Insert();
                    }
                }

                return new ResponseImage()
                {
                    URL = fileUrl,
                    Source = $"https://danbooru.donmai.us/posts/{result.Id}",
                    Description = descriptionString,
                    Referrer = "danbooru.donmai.us",
                };
            }
            else return new ResponseImage();
        }
    }
}
