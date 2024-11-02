using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("ai_prompt_rules")]
public class AiPromptRule(string rule) : BaseDbEntity
{
    [Column("rule_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RuleId { get; set; }

    [Column("rule")]
    public string Rule { get; set; } = rule;
}