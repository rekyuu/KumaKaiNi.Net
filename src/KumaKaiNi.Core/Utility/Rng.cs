namespace KumaKaiNi.Core.Utility;

public static class Rng
{
    public static T PickRandom<T>(IEnumerable<T> choices)
    {
        Random rng = new();
        IEnumerable<T> enumerable = choices as T[] ?? choices.ToArray();
        int result = rng.Next(0, enumerable.Count());

        return enumerable.ElementAt(result);
    }

    public static bool OneTo(int odds)
    {
        Random rng = new();
        int result = rng.Next(0, odds);

        return result == 0;
    }
}