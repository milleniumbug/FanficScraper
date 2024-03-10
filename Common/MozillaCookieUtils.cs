﻿using System.Net;

namespace Common;

public class MozillaCookieUtils
{
    public static void WriteCookiesInMozillaFormat(
        Stream cookiesFs,
        IReadOnlyList<Cookie>? cookies)
    {
        using var writer = new StreamWriter(cookiesFs, leaveOpen: true);
        writer.NewLine = "\n";
        WriteCookiesInMozillaFormat(writer, cookies);
        writer.Flush();
    }
    
    public static void WriteCookiesInMozillaFormat(
        TextWriter writer,
        IReadOnlyList<Cookie>? cookies)
    {
        if (cookies != null)
        {
            writer.WriteLine("# HTTP Cookie File");
            foreach (var cookie in cookies)
            {
                writer.WriteLine(StringFromCookie(cookie));
            }
        }
    }

    public static string StringFromCookies(IReadOnlyList<Cookie> cookies)
    {
        using var stringWriter = new StringWriter();
        WriteCookiesInMozillaFormat(stringWriter, cookies);
        return stringWriter.ToString();
    }

    public static string StringFromCookie(Cookie cookie)
    {
        // seems to be irrelevant nowadays, we need to shut up the Python cookiejar assert here
        bool includeSubdomains = cookie.Domain.StartsWith(".", StringComparison.Ordinal);
        return
            $"{cookie.Domain}\t{(includeSubdomains ? "TRUE" : "FALSE")}\t{cookie.Path}\t{(cookie.Secure ? "TRUE" : "FALSE")}\t{new DateTimeOffset(cookie.Expires).ToUnixTimeSeconds()}\t{cookie.Name}\t{cookie.Value}";
    }
}