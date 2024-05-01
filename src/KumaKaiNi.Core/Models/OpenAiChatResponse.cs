using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class OpenAiChatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("choices")]
    public OpenAiChatChoice[]? Choices { get; set; }
    
    [JsonPropertyName("created")]
    public int? Created { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
    
    [JsonPropertyName("object")]
    public string? ObjectType { get; set; }
    
    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; set; }
}

public class OpenAiChatChoice
{
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
    
    [JsonPropertyName("index")]
    public int? Index { get; set; }
    
    [JsonPropertyName("message")]
    public OpenAiChatMessage? Message { get; set; }
    
    [JsonPropertyName("logprobs")]
    public OpenAiLogProbsContent? LogProbs { get; set; }
}

public class OpenAiLogProbsContent
{
    [JsonPropertyName("content")]
    public OpenAiLogProbWithTop[]? Content { get; set; }
}

public class OpenAiLogProbWithTop : OpenAiLogProb
{
    [JsonPropertyName("top_logprobs")]
    public OpenAiLogProb[]? TopLogProbs { get; set; }
}

public class OpenAiLogProb
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    
    [JsonPropertyName("logprob")]
    public double? LogProb { get; set; }
    
    [JsonPropertyName("bytes")]
    public byte[]? Bytes { get; set; }
}

public class OpenAiUsage
{
    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }
    
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; set; }
}