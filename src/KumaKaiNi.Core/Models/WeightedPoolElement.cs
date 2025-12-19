namespace KumaKaiNi.Core.Models;

public class WeightedPoolElement<T>(T element, long weight)
{
    public long Weight { get; set; } = weight;

    public T Element { get; set; } = element;

    public long AccumulatedWeight { get; set; } = 0;
}