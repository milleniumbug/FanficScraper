using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FanficScraper.Data;

public class StringArrayConverter : ValueConverter<IReadOnlyList<string>, string>
{
    public StringArrayConverter() :
        base(
            v => string.Join("\u0001", v),
            v => v.Split('\u0001', StringSplitOptions.None))
    {
    }
}