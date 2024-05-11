using KumaKaiNi.Core.Models;

namespace KumaKaiNi.Core.Attributes;

/// <summary>
/// Base class for bot message parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class BaseResponseAttribute : Attribute
{
    /// <summary>
    /// The values to look for when parsing messages.
    /// </summary>
    public string[] Values { get; private set; }
    
    /// <summary>
    /// The minimum required user authority to process the message.
    /// </summary>
    public UserAuthority UserAuthority { get; private set; }
    
    /// <summary>
    /// Require the channel to be NSFW enabled or not.
    /// </summary>
    public bool Nsfw { get; private set; }

    /// <summary>
    /// Base class for bot message parsing.
    /// </summary>
    /// <param name="value">The value to look for when parsing messages.</param>
    /// <param name="userAuthority">The minimum required user authority to process the message.</param>
    /// <param name="nsfw">Require the channel to be NSFW enabled or not.</param>
    protected BaseResponseAttribute(string value, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false)
    {
        Values = [value];
        UserAuthority = userAuthority;
        Nsfw = nsfw;
    }

    /// <summary>
    /// Base class for bot message parsing.
    /// </summary>
    /// <param name="values">The values to look for when parsing messages.</param>
    /// <param name="userAuthority">The minimum required user authority to process the message.</param>
    /// <param name="nsfw">Require the channel to be NSFW enabled or not.</param>
    protected BaseResponseAttribute(string[] values, UserAuthority userAuthority = UserAuthority.User, bool nsfw = false)
    {
        Values = values;
        UserAuthority = userAuthority;
        Nsfw = nsfw;
    }
}