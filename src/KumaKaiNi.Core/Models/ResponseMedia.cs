using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

/// <summary>
/// Object used for including images in responses.
/// </summary>
/// <param name="url">The direct URL to the image.</param>
/// <param name="source">Where the image was found.</param>
/// <param name="description">Description of the image.</param>
/// <param name="referrer">The host of the image.</param>
public class ResponseMedia(string url, string preview, string source, string description, string referrer)
{
    /// <summary>
    /// The direct URL to the media.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = url;

    /// <summary>
    /// The direct URL to the media.
    /// </summary>
    [JsonPropertyName("preview")]
    public string Preview { get; set; } = preview;

    /// <summary>
    /// Where the image was found.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = source;

    /// <summary>
    /// Description of the image.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = description;

    /// <summary>
    /// The host of the image.
    /// </summary>
    [JsonPropertyName("referrer")]
    public string Referrer { get; set; } = referrer;
}