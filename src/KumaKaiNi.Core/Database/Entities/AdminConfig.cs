using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

public class AdminConfig : BaseDbEntity
{
    [Column("open_ai_model")]
    public string OpenAiModel { get; set; } = "gpt-4-turbo";

    [Column("open_ai_token_limit")]
    public long OpenAiTokenLimit { get; set; } = 2048;

    [Column("open_ai_initial_prompt")]
    public string AiInitialPrompt { get; set; } =
        """
        You are a chat bot named after the Japanese battleship, Kuma. Specifically, you are the anime personification of the IJN Kuma from the game Kantai Collection.

        Messages will be provided as a recent message history from multiple users, and you should respond considering the context of these messages. When responding, you must obey the following rules:
        """;
}