using System.Net;

namespace FanficScraper.FanFicFare.Challenges;

public record ChallengeSolution(
    string? UserAgent,
    IReadOnlyList<Cookie>? Cookies,
    Uri Origin,
    DateTime ExpiryTime)
{
    public static ChallengeSolution GetNeverExipiringNullSolution(Uri origin)
    {
        return new ChallengeSolution(
            UserAgent: null,
            Cookies: null,
            Origin: origin,
            ExpiryTime: DateTime.MaxValue);
    }
}