using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class OllamaChatRequest(List<OpenAiChatMessage> messages, string model = "llama3")
{
    [JsonPropertyName("messages")]
    public List<OpenAiChatMessage> Messages { get; set; } = messages;
    
    [JsonPropertyName("model")]
    public string Model { get; set; } = model;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("options")]
    public OllamaModelOptions Options { get; set; } = new();
}

public class OllamaModelOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.8f;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.95f;

    [JsonPropertyName("top_k")]
    public double TopK { get; set; } = 40;

    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; } = 0;

    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0;

    [JsonPropertyName("repeat_penalty")]
    public double RepeatPenalty { get; set; } = 1.1f;

    [JsonPropertyName("seed")]
    public long Seed { get; set; } = -1;
}