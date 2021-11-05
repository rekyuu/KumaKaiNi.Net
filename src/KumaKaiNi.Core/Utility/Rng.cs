using System;
using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public static class Rng
    {
        public static T PickRandom<T>(List<T> choices)
        {
            return PickRandom(choices.ToArray());
        }

        public static T PickRandom<T>(T[] choices)
        {
            Random rng = new Random();
            int result = rng.Next(0, choices.Length);

            return choices[result];
        }

        public static bool OneTo(int odds)
        {
            Random rng = new Random();
            int result = rng.Next(0, odds);

            return result == 0;
        }
    }
}
