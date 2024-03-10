using System.Text;
using System.Web;

namespace Common.Utils;

public class QueryStringBuilder
{
    private readonly StringBuilder builder = new StringBuilder();
    
    public static string Create(FormattableString str)
    {
        throw new NotImplementedException();
    }

    public static string Create(IEnumerable<(string? key, string? value)> components)
    {
        var b = new QueryStringBuilder();
        foreach (var (key, value) in components)
        {
            b.AddIfNotNull(key, value);
        }

        return b.ToString();
    }

    public QueryStringBuilder Add(string key, string? value = null)
    {
        builder.Append(builder.Length == 0 ? '?' : '&');
        builder.Append(HttpUtility.UrlEncode(key));
        if (value != null)
        {
            builder.Append('=');
            builder.Append(HttpUtility.UrlEncode(value));
        }

        return this;
    }
    
    public QueryStringBuilder AddIf(bool condition, string? key, string? value)
    {
        if (condition && key != null)
        {
            Add(key, value);
        }
        
        return this;
    }
    
    public QueryStringBuilder AddIfNotNull(string? key, string? value)
    {
        if (key != null && value != null)
        {
            Add(key, value);
        }
        
        return this;
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}