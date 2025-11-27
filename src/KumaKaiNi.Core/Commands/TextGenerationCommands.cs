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
    [Command("gdq")]
    public static async Task<KumaResponse?> GdqAsync()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "https://taskinoz.com/gdq/api/");
        HttpResponseMessage response = await Rest.SendAsync(request);
        
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync();

        return !string.IsNullOrEmpty(content) ? new KumaResponse(content) : null;
    }
}
