using KumaKaiNi.Core.Models;

namespace KumaKaiNi.Core.Attributes;

/// <summary>
/// Used to specify any word or phrase within a message.
/// </summary>
public class PhraseAttribute : BaseResponseAttribute
{
    /// <summary>
    /// Used to specify any word or phrase within a message.
    /// </summary>
    /// <inheritdoc />
    public PhraseAttribute(string value, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false) 
        : base(value, userAuthority, nsfw) { }

    /// <summary>
    /// Used to specify any words or phrases within a message.
    /// </summary>
    /// <inheritdoc />
    public PhraseAttribute(string[] values, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false)
        : base(values, userAuthority, nsfw) { }
}