using System.Net;

namespace Common.Challenges;

public record ChallengeSolution(
    string? UserAgent,
    IReadOnlyList<Cookie>? Cookies,
    Uri Origin,
    DateTime ExpiryTime)
{
    public static ChallengeSolution GetNeverExpiringNullSolution(Uri origin)
    {
        return new ChallengeSolution(
            UserAgent: null,
            Cookies: null,
            Origin: origin,
            ExpiryTime: DateTime.MaxValue);
    }
}