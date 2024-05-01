using KumaKaiNi.Core.Models;

namespace KumaKaiNi.Core.Attributes;

/// <summary>
/// Used to specify a command. Commands are defined as the first word of a message, prefixed by a !
/// </summary>
public class CommandAttribute : BaseResponseAttribute
{
    /// <summary>
    /// Used to specify a command. Commands are defined as the first word of a message, prefixed by a !
    /// </summary>
    /// <inheritdoc />
    public CommandAttribute(string value, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false) 
        : base(value, userAuthority, nsfw) { }

    /// <summary>
    /// Used to specify commands. Commands are defined as the first word of a message, prefixed by a !
    /// </summary>
    /// <inheritdoc />
    public CommandAttribute(string[] values, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false)
        : base(values, userAuthority, nsfw) { }
}