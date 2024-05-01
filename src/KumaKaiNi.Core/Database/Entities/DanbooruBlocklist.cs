using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("danbooru_blocklists")]
public class DanbooruBlockList(string tag) : BaseDbEntity
{
    [Column("tag")]
    public string Tag { get; set; } = tag;
}
