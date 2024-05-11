using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace KumaKaiNi.Core.Utility;

public static class Rest
{
    private static readonly HttpClient Client;
    
    static Rest()
    {
        Client = new HttpClient();

        string userAgentName = KumaConfig.ApplicationName
            .Replace(" ", "")
            .Replace("-", "");
        
        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(userAgentName, KumaConfig.ApplicationVersion));
        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"({RuntimeInformation.OSDescription}: {RuntimeInformation.OSArchitecture})"));
        Client.Timeout = TimeSpan.FromMinutes(5);
    }

    public static HttpResponseMessage Send(HttpRequestMessage request)
    {
        return Client.Send(request);
    }

    public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return await Client.SendAsync(request);
    }
}