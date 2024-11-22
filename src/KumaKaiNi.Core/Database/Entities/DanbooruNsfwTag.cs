using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("danbooru_nsfw_tags")]
public class DanbooruNsfwTag(string tag) : BaseDbEntity
{
    [Column("tag")]
    public string Tag { get; set; } = tag;
}
