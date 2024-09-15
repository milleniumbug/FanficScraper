namespace Common;

public static class UriExtensions
{
    public static string GetOrigin(this Uri uri)
    {
        return GetOriginUri(uri).ToString();
    }
    
    public static Uri GetOriginUri(this Uri uri)
    {
        return new Uri(uri.GetLeftPart(UriPartial.Authority));
    }
}