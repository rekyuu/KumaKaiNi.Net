using System;
using Newtonsoft.Json;
using System.Net.Http;

namespace KumaKaiNi.Core
{
    public static class Images
    {
        [Command("smug")]
        public static Response Smug()
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Client-ID {Config.ImgurClientId}");
            HttpResponseMessage request = client.GetAsync("https://api.imgur.com/3/album/zSNC1").Result;
            ImgurResults response = JsonConvert.DeserializeObject<ImgurResults>(request.Content.ReadAsStringAsync().Result);
            ImgurImage result = Rng.PickRandom(response?.Data.Images);

            ResponseImage image = new ResponseImage()
            {
                Url = result.Link.ToString(),
                Source = $"https://imgur.com/{result.Id}",
                Description = "",
                Referrer = "imgur.com"
            };

            return new Response(image);
        }
    }
}
