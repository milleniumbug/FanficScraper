using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FanficScraper.Data;

public class StringArrayComparer : ValueComparer<IReadOnlyList<string>>
{
    public StringArrayComparer() :
        base(
            (l, r) => 
                (l == null && r == null) ||
                (l != null && r != null && l.SequenceEqual(r)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())))
    {
    }
}