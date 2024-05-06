using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("quotes")]
public class Quote(string text) : BaseDbEntity
{
    [Column("quote_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long QuoteId { get; set; }
    
    [Column("text")]
    public string Text { get; set; } = text;
}
