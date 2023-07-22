using System.Net;

namespace Common;

public class MozillaCookieUtils
{
    public static void WriteCookiesInMozillaFormat(
        Stream cookiesFs,
        IReadOnlyList<Cookie>? cookies)
    {
        using var writer = new StreamWriter(cookiesFs, leaveOpen: true)
        {
            NewLine = "\n"
        };
        if (cookies != null)
        {
            writer.WriteLine("# HTTP Cookie File");
            foreach (var cookie in cookies)
            {
                // seems to be irrelevant nowadays, we need to shut up the Python cookiejar assert here
                bool includeSubdomains = cookie.Domain.StartsWith(".", StringComparison.Ordinal);
                writer.WriteLine($"{cookie.Domain}\t{(includeSubdomains ? "TRUE" : "FALSE")}\t{cookie.Path}\t{(cookie.Secure ? "TRUE" : "FALSE")}\t{new DateTimeOffset(cookie.Expires).ToUnixTimeSeconds()}\t{cookie.Name}\t{cookie.Value}");
            }
        }
        writer.Flush();
    }
}