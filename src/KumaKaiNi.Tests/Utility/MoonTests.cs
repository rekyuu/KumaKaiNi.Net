using KumaKaiNi.Core.Utility;

namespace KumaKaiNi.Tests.Utility;

public class MoonTests
{
    [Fact]
    public void IsBlueMoon_IsTrueOnCorrectDates()
    {
        DateTime fullMoon = new(2026, 5, 1);
        DateTime newMoon = new(2026, 5, 17);
        DateTime blueMoon = new(2026, 5, 31);

        Assert.False(Moon.IsBlueMoon(fullMoon));
        Assert.False(Moon.IsBlueMoon(newMoon));
        Assert.True(Moon.IsBlueMoon(blueMoon));
    }
}