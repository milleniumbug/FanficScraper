using System.Text.RegularExpressions;
using System.Web;
using Common;

namespace FanficScraper.FanFicFare;

// lightweight alternative to calling FanFicFare directly to know the "canonical" story url
// falls back to calling FanFicFare if it doesn't know what to do with the URL
public class StoryUrlNormalizer
{
    private readonly IFanFicFare fanFicFare;
    private readonly HttpClient httpClient;

    public StoryUrlNormalizer(
        IFanFicFare fanFicFare,
        HttpClient httpClient)
    {
        this.fanFicFare = fanFicFare;
        this.httpClient = httpClient;
    }

    public static string? GetAbbreviation(Uri uri)
    {
        switch (uri.HostNoWww())
        {
            case "scribblehub.com":
                return "scrhub";
            case "tgstorytime.com":
                return "tgstory";
            case "archiveofourown.org":
                return "ao3";
            case "royalroad.com":
                return "rylrdl";
            case "wattpad.com":
                return "wattpad";
            default:
                return null;
        }
    }

    public async Task<Uri> NormalizeUrl(Uri uri)
    {
        switch (GetAbbreviation(uri))
        {
            case "scrhub":
                return await ScribbleHub(uri);
            case "tgstory":
                return await TgStoryTime(uri);
            case "ao3":
                return await ArchiveOfOurOwn(uri);
            case "rylrdl":
                return await RoyalRoad(uri);
            default:
                return await GetFallback(uri);
        }
    }

    private async Task<Uri> RoyalRoad(Uri uri)
    {
        var trimmed = uri.GetLeftPart(UriPartial.Path);
        var match = new Regex(@"^https?://(www\.)?royalroad\.com/fiction/([0-9]+)").Match(trimmed);
        if (!match.Success)
        {
            return await GetFallback(uri);
        }

        var worksParameter = match.Groups[2].Value;
        return new Uri($"https://www.royalroad.com/fiction/{worksParameter}");
    }

    private async Task<Uri> ArchiveOfOurOwn(Uri uri)
    {
        var trimmed = uri.GetLeftPart(UriPartial.Path);
        var match = new Regex(@"^https?://(www\.)?archiveofourown\.org/works/([0-9]+)").Match(trimmed);
        if (!match.Success)
        {
            return await GetFallback(uri);
        }

        var worksParameter = match.Groups[2].Value;
        return new Uri($"https://archiveofourown.org/works/{worksParameter}");
    }

    private async Task<Uri> GetFallback(Uri uri)
    {
        var fanFicFareDetails = await this.fanFicFare.Run(uri, metadataOnly: true);
        return new Uri(fanFicFareDetails.StoryUrl);
    }

    private async Task<Uri> ScribbleHub(Uri uri)
    {
        uri = new UriBuilder(uri)
        {
            Scheme = "https",
            Port = 443,
        }.Uri;
        var trimmed = uri.GetLeftPart(UriPartial.Path);
        var match = new Regex(@"^(https://(www\.)?scribblehub\.com/series/[0-9]+)").Match(trimmed);
        if (!match.Success)
        {
            return await GetFallback(uri);
        }
        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, match.Captures[0].Value));
        return response.RequestMessage!.RequestUri!;
    }
    
    private async Task<Uri> TgStoryTime(Uri uri)
    {
        var originalUri = uri;
        var uriBuilder = new UriBuilder(uri)
        {
            Scheme = "http",
            Port = 80,
        };
        uri = uriBuilder.Uri;
        var trimmed = uri.GetLeftPart(UriPartial.Query);
        var match = new Regex(@"^(http://(www\.)?tgstorytime\.com/viewstory.php)").Match(trimmed);
        if (!match.Success)
        {
            return await GetFallback(originalUri);
        }

        var sid = HttpUtility.ParseQueryString(uri.Query)["sid"];
        if (sid == null)
        {
            return await GetFallback(originalUri);
        }

        return new Uri($"http://tgstorytime.com/viewstory.php?sid={sid}");
    }
}