using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("quotes")]
public class Quote(string text) : BaseDbEntity
{
    [Column("text")]
    public string Text { get; set; } = text;
}
