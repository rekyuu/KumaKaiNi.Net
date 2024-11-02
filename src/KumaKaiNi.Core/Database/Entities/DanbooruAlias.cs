using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("danbooru_aliases")]
public class DanbooruAlias(string alias, string tag) : BaseDbEntity
{
    [Column("alias")]
    public string Alias { get; set; } = alias;

    [Column("tag")]
    public string Tag { get; set; } = tag;
}