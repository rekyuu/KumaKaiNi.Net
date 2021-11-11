using System;

namespace KumaKaiNi.Core
{
    public class ImgurResults
    {
        public ImgurData Data { get; set; }
        public bool Success { get; set; }
        public long Status { get; set; }
    }

    public class ImgurData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public object Description { get; set; }
        public long Datetime { get; set; }
        public string Cover { get; set; }
        public object CoverEdited { get; set; }
        public long CoverWidth { get; set; }
        public long CoverHeight { get; set; }
        public object AccountUrl { get; set; }
        public object AccountId { get; set; }
        public string Privacy { get; set; }
        public string Layout { get; set; }
        public long Views { get; set; }
        public Uri Link { get; set; }
        public bool Favorite { get; set; }
        public bool Nsfw { get; set; }
        public string Section { get; set; }
        public long ImagesCount { get; set; }
        public bool InGallery { get; set; }
        public bool IsAd { get; set; }
        public bool IncludeAlbumAds { get; set; }
        public bool IsAlbum { get; set; }
        public ImgurImage[] Images { get; set; }
        public ImgurAdConfig AdConfig { get; set; }
    }

    public class ImgurAdConfig
    {
        public string[] SafeFlags { get; set; }
        public object[] HighRiskFlags { get; set; }
        public string[] UnsafeFlags { get; set; }
        public object[] WallUnsafeFlags { get; set; }
        public bool ShowsAds { get; set; }
    }

    public class ImgurImage
    {
        public string Id { get; set; }
        public object Title { get; set; }
        public object Description { get; set; }
        public long Datetime { get; set; }
        public string Type { get; set; }
        public bool Animated { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }
        public long Size { get; set; }
        public long Views { get; set; }
        public long Bandwidth { get; set; }
        public object Vote { get; set; }
        public bool Favorite { get; set; }
        public object Nsfw { get; set; }
        public object Section { get; set; }
        public object AccountUrl { get; set; }
        public object AccountId { get; set; }
        public bool IsAd { get; set; }
        public bool InMostViral { get; set; }
        public bool HasSound { get; set; }
        public object[] Tags { get; set; }
        public long AdType { get; set; }
        public string AdUrl { get; set; }
        public long Edited { get; set; }
        public bool InGallery { get; set; }
        public Uri Link { get; set; }
    }
}
