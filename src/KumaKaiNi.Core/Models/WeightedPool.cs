namespace KumaKaiNi.Core.Models;

public class WeightedPool<T>
{
    public List<WeightedPoolElement<T>> Elements { get; set; } = [];

    public long TotalWeight { get; private set; } = 0;

    public void UpdateWeights()
    {
        TotalWeight = 0;
        Elements = Elements.OrderBy(x => x.Weight).ToList();

        foreach (WeightedPoolElement<T> element in Elements)
        {
            TotalWeight += element.Weight;
            element.AccumulatedWeight = TotalWeight;
        }
    }

    public void AddElement(T element, long weight)
    {
        WeightedPoolElement<T> poolElement = new(element, weight);
        Elements.Add(poolElement);
    }

    public T GetRandomElement()
    {
        Random r = new();
        float result = r.NextSingle() * TotalWeight;

        return Elements.First(x => x.AccumulatedWeight >= result).Element;
    }
}