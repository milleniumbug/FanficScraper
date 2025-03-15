using Common.Utils;

namespace Common;

public static class UriExtensions
{
    public static string GetOrigin(this Uri uri)
    {
        return GetOriginUri(uri).ToString();
    }
    
    public static string HostNoWww(this Uri uri)
    {
        return uri.Host.TryTrimStart("www.", StringComparison.Ordinal, out var urlNoWww) ? urlNoWww : uri.Host;
    }
    
    public static Uri GetOriginUri(this Uri uri)
    {
        return new Uri(uri.GetLeftPart(UriPartial.Authority));
    }
}