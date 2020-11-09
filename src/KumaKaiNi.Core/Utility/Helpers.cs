using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KumaKaiNi.Core
{
    public static class Helpers
    {
        public static string ToSnakeCase(string text)
        {
            Regex regex = new Regex(@"((?<=.)[A-Z][a-zA-Z]*)|((?<=[a-zA-Z])\d+)", RegexOptions.Multiline);
            return regex.Replace(text, @"_$1$2").ToLower();
        }
    }
}
