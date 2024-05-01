using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class OpenAiChatMessage(string content, string role)
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = content;

    [JsonPropertyName("role")]
    public string Role { get; set; } = role;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}