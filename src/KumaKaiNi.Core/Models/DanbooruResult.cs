using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class DanbooruResult
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("uploader_id")]
    public long? UploaderId { get; set; }

    [JsonPropertyName("score")]
    public long? Score { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }

    [JsonPropertyName("last_comment_bumped_at")]
    public DateTimeOffset? LastCommentBumpedAt { get; set; }

    [JsonPropertyName("rating")]
    public string? Rating { get; set; }

    [JsonPropertyName("image_width")]
    public long? ImageWidth { get; set; }

    [JsonPropertyName("image_height")]
    public long? ImageHeight { get; set; }

    [JsonPropertyName("tag_string")]
    public string? TagString { get; set; }

    [JsonPropertyName("is_note_locked")]
    public bool? IsNoteLocked { get; set; }

    [JsonPropertyName("fav_count")]
    public long? FavCount { get; set; }

    [JsonPropertyName("file_ext")]
    public string? FileExt { get; set; }

    [JsonPropertyName("last_noted_at")]
    public DateTimeOffset? LastNotedAt { get; set; }

    [JsonPropertyName("is_rating_locked")]
    public bool? IsRatingLocked { get; set; }

    [JsonPropertyName("parent_id")]
    public long? ParentId { get; set; }

    [JsonPropertyName("has_children")]
    public bool? HasChildren { get; set; }

    [JsonPropertyName("approver_id")]
    public long? ApproverId { get; set; }

    [JsonPropertyName("tag_count_general")]
    public long? TagCountGeneral { get; set; }

    [JsonPropertyName("tag_count_artist")]
    public long? TagCountArtist { get; set; }

    [JsonPropertyName("tag_count_character")]
    public long? TagCountCharacter { get; set; }

    [JsonPropertyName("tag_count_copyright")]
    public long? TagCountCopyright { get; set; }

    [JsonPropertyName("file_size")]
    public long? FileSize { get; set; }

    [JsonPropertyName("is_status_locked")]
    public bool? IsStatusLocked { get; set; }

    [JsonPropertyName("pool_string")]
    public string? PoolString { get; set; }

    [JsonPropertyName("up_score")]
    public long? UpScore { get; set; }

    [JsonPropertyName("down_score")]
    public long? DownScore { get; set; }

    [JsonPropertyName("is_pending")]
    public bool? IsPending { get; set; }

    [JsonPropertyName("is_flagged")]
    public bool? IsFlagged { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool? IsDeleted { get; set; }

    [JsonPropertyName("tag_count")]
    public long? TagCount { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("is_banned")]
    public bool? IsBanned { get; set; }

    [JsonPropertyName("pixiv_id")]
    public long? PixivId { get; set; }

    [JsonPropertyName("last_commented_at")]
    public DateTimeOffset? LastCommentedAt { get; set; }

    [JsonPropertyName("has_active_children")]
    public bool? HasActiveChildren { get; set; }

    [JsonPropertyName("bit_flags")]
    public long? BitFlags { get; set; }

    [JsonPropertyName("tag_count_meta")]
    public long? TagCountMeta { get; set; }

    [JsonPropertyName("has_large")]
    public bool? HasLarge { get; set; }

    [JsonPropertyName("has_visible_children")]
    public bool? HasVisibleChildren { get; set; }

    [JsonPropertyName("is_favorited")]
    public bool? IsFavorited { get; set; }

    [JsonPropertyName("tag_string_general")]
    public string? TagStringGeneral { get; set; }

    [JsonPropertyName("tag_string_character")]
    public string? TagStringCharacter { get; set; }

    [JsonPropertyName("tag_string_copyright")]
    public string? TagStringCopyright { get; set; }

    [JsonPropertyName("tag_string_artist")]
    public string? TagStringArtist { get; set; }

    [JsonPropertyName("tag_string_meta")]
    public string? TagStringMeta { get; set; }

    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("large_file_url")]
    public string? LargeFileUrl { get; set; }

    [JsonPropertyName("preview_file_url")]
    public string? PreviewFileUrl { get; set; }
}