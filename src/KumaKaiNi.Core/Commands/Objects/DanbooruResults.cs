using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public partial class DanbooruResults
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("uploader_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? UploaderId { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public long? Score { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }

        [JsonProperty("md5", NullValueHandling = NullValueHandling.Ignore)]
        public string Md5 { get; set; }

        [JsonProperty("last_comment_bumped_at")]
        public DateTimeOffset? LastCommentBumpedAt { get; set; }

        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
        public string Rating { get; set; }

        [JsonProperty("image_width", NullValueHandling = NullValueHandling.Ignore)]
        public long? ImageWidth { get; set; }

        [JsonProperty("image_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? ImageHeight { get; set; }

        [JsonProperty("tag_string", NullValueHandling = NullValueHandling.Ignore)]
        public string TagString { get; set; }

        [JsonProperty("is_note_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNoteLocked { get; set; }

        [JsonProperty("fav_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? FavCount { get; set; }

        [JsonProperty("file_ext", NullValueHandling = NullValueHandling.Ignore)]
        public string FileExt { get; set; }

        [JsonProperty("last_noted_at")]
        public DateTimeOffset? LastNotedAt { get; set; }

        [JsonProperty("is_rating_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRatingLocked { get; set; }

        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("has_children", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasChildren { get; set; }

        [JsonProperty("approver_id")]
        public long? ApproverId { get; set; }

        [JsonProperty("tag_count_general", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCountGeneral { get; set; }

        [JsonProperty("tag_count_artist", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCountArtist { get; set; }

        [JsonProperty("tag_count_character", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCountCharacter { get; set; }

        [JsonProperty("tag_count_copyright", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCountCopyright { get; set; }

        [JsonProperty("file_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? FileSize { get; set; }

        [JsonProperty("is_status_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsStatusLocked { get; set; }

        [JsonProperty("pool_string", NullValueHandling = NullValueHandling.Ignore)]
        public string PoolString { get; set; }

        [JsonProperty("up_score", NullValueHandling = NullValueHandling.Ignore)]
        public long? UpScore { get; set; }

        [JsonProperty("down_score", NullValueHandling = NullValueHandling.Ignore)]
        public long? DownScore { get; set; }

        [JsonProperty("is_pending", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPending { get; set; }

        [JsonProperty("is_flagged", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFlagged { get; set; }

        [JsonProperty("is_deleted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDeleted { get; set; }

        [JsonProperty("tag_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCount { get; set; }

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("is_banned", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsBanned { get; set; }

        [JsonProperty("pixiv_id")]
        public long? PixivId { get; set; }

        [JsonProperty("last_commented_at")]
        public DateTimeOffset? LastCommentedAt { get; set; }

        [JsonProperty("has_active_children", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasActiveChildren { get; set; }

        [JsonProperty("bit_flags", NullValueHandling = NullValueHandling.Ignore)]
        public long? BitFlags { get; set; }

        [JsonProperty("tag_count_meta", NullValueHandling = NullValueHandling.Ignore)]
        public long? TagCountMeta { get; set; }

        [JsonProperty("has_large", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasLarge { get; set; }

        [JsonProperty("has_visible_children", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasVisibleChildren { get; set; }

        [JsonProperty("is_favorited", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFavorited { get; set; }

        [JsonProperty("tag_string_general", NullValueHandling = NullValueHandling.Ignore)]
        public string TagStringGeneral { get; set; }

        [JsonProperty("tag_string_character", NullValueHandling = NullValueHandling.Ignore)]
        public string TagStringCharacter { get; set; }

        [JsonProperty("tag_string_copyright", NullValueHandling = NullValueHandling.Ignore)]
        public string TagStringCopyright { get; set; }

        [JsonProperty("tag_string_artist", NullValueHandling = NullValueHandling.Ignore)]
        public string TagStringArtist { get; set; }

        [JsonProperty("tag_string_meta", NullValueHandling = NullValueHandling.Ignore)]
        public string TagStringMeta { get; set; }

        [JsonProperty("file_url", NullValueHandling = NullValueHandling.Ignore)]
        public string FileUrl { get; set; }

        [JsonProperty("large_file_url", NullValueHandling = NullValueHandling.Ignore)]
        public string LargeFileUrl { get; set; }

        [JsonProperty("preview_file_url", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewFileUrl { get; set; }
    }
}
