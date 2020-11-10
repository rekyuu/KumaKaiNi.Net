using System;
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

        public static int GetMoonPhase(DateTime date)
        {
            (int year, int month, int day) = (date.Year, date.Month, date.Day);

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            int a = (int)(year / 100f);
            int b = (int)(a / 4f);
            int c = 2 - a + b;
            int e = (int)(365.25f * (year + 4716f));
            int f = (int)(30.6001f * (month + 1f));

            float cycleLength = 29.53f;
            float julianDate = c + day + e + f - 1524.5f;
            float daysSinceNewMoon = (int)(julianDate - 2451549.5f);
            float newMoons = daysSinceNewMoon / cycleLength;

            return (int)((newMoons - (int)newMoons) * cycleLength);
        }
    }
}
