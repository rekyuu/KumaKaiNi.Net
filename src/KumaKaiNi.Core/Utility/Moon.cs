namespace KumaKaiNi.Core.Utility;

public static class Moon
{
    /// <summary>
    /// Gets the moon phase of the provided date.
    /// </summary>
    /// <param name="date">The date of the moon phase.</param>
    /// <returns>The moon phase as an <see langword="int" />.</returns>
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

        const float cycleLength = 29.53f;
        float julianDate = c + day + e + f - 1524.5f;
        float daysSinceNewMoon = (int)(julianDate - 2451549.5f);
        float newMoons = daysSinceNewMoon / cycleLength;

        return (int)((newMoons - (int)newMoons) * cycleLength);
    }

    /// <summary>
    /// Checks if the supplied date is the second full moon of the month.
    /// </summary>
    /// <param name="date">The date of the moon phase.</param>
    /// <returns>
    /// <see langword="true"/> if it's the second full moon of the month, <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsBlueMoon(DateTime date)
    {
        int currentPhase = GetMoonPhase(date);
        if (currentPhase != 15) return false;

        // Check if a full moon has happened this month already
        for (int day = 1; day < date.Day; day++)
        {
            DateTime dayOfMonth = new(date.Year, date.Month, day);
            int phase = GetMoonPhase(dayOfMonth);

            if (phase == 15) return true;
        }

        return false;
    }
}