using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FanficScraper.Data;

public class StringArrayConverter : ValueConverter<IReadOnlyList<string>, string>
{
    public StringArrayConverter() :
        base(
            v => string.Join("\0", v),
            v => v.Split('\0', StringSplitOptions.None))
    {
    }
}