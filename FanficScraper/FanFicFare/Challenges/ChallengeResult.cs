using System.Net;

namespace FanficScraper.FanFicFare.Challenges;

public record ChallengeResult(
    string? UserAgent,
    IReadOnlyList<Cookie>? Cookies,
    Uri Origin,
    DateTime ExpiryTime);