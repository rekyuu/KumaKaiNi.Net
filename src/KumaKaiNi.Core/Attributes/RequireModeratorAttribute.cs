﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequireModeratorAttribute : Attribute
    {
        public RequireModeratorAttribute() { }
    }
}
