using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class OpenAiChatRequest(List<OpenAiChatMessage> messages, string model = "gpt-4")
{
    [JsonPropertyName("messages")]
    public List<OpenAiChatMessage> Messages { get; set; } = messages;

    [JsonPropertyName("model")]
    public string Model { get; set; } = model;
    
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }
    
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int>? LogitBias { get; set; }
    
    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }
    
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
    
    [JsonPropertyName("n")]
    public int? N { get; set; }
    
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }
    
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }
    
    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }
    
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
    
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }
    
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }
    
    [JsonPropertyName("user")]
    public string? User { get; set; }
}