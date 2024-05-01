namespace KumaKaiNi.Core.Models;

/// <summary>
/// Object used for including images in responses.
/// </summary>
/// <param name="url">The direct URL to the image.</param>
/// <param name="source">Where the image was found.</param>
/// <param name="description">Description of the image.</param>
/// <param name="referrer">The host of the image.</param>
public class ResponseImage(string url, string source, string description, string referrer)
{
    /// <summary>
    /// The direct URL to the image.
    /// </summary>
    public string Url { get; set; } = url;

    /// <summary>
    /// Where the image was found.
    /// </summary>
    public string Source { get; set; } = source;

    /// <summary>
    /// Description of the image.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// The host of the image.
    /// </summary>
    public string Referrer { get; set; } = referrer;
}