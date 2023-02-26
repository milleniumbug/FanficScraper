namespace FanficScraper.Utils;

public static class UriExtensions
{
    public static string GetOrigin(this Uri uri)
    {
        return new Uri(uri.GetLeftPart(UriPartial.Authority)).ToString();
    }
}