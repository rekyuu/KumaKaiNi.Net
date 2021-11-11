using System;

namespace KumaKaiNi.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequireAdminAttribute : Attribute { }
}
