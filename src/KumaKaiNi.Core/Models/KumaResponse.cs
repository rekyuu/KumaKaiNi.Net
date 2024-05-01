namespace KumaKaiNi.Core.Models;

public class KumaResponse
{
    /// <summary>
    /// The response message.
    /// </summary>
    public string? Message  { get; set; }
    
    /// <summary>
    /// The response image.
    /// </summary>
    public ResponseImage? Image  { get; set; }
    
    /// <summary>
    /// The response destination channel ID.
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="message">The response message.</param>
    public KumaResponse(string? message = "")
    {
        Message = message;
    }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="image">The response image.</param>
    public KumaResponse(ResponseImage image)
    {
        Message = "";
        Image = image;
    }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="message">The response message.</param>
    /// <param name="image">The response image.</param>
    public KumaResponse(string? message, ResponseImage image)
    {
        Message = message;
        Image = image;
    }

    public override string? ToString()
    {
        if (string.IsNullOrEmpty(Message) && Image != null) return Image.Source;
        return Message;
    }
}