using System.Collections;

namespace FanficScraper.Utils;

public class ToStringableReadOnlyList<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T> list;

    public ToStringableReadOnlyList(IReadOnlyList<T> list)
    {
        this.list = list;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)list).GetEnumerator();
    }

    public int Count => list.Count;

    public T this[int index] => list[index];

    public override string ToString()
    {
        return "[" + string.Join(";", this.list) + "]";
    }
}