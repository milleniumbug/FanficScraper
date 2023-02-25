using System.Collections.Concurrent;

namespace FanficScraper.FanFicFare.Challenges;

public class CachingChallengeSolver : IChallengeSolver
{
    private readonly IChallengeSolver solver;
    private readonly ConcurrentDictionary<string, Task<ChallengeResult>> challengeResults = new();
    private readonly TimeSpan tolerancePeriod;

    public CachingChallengeSolver(
        IChallengeSolver solver,
        TimeSpan? tolerancePeriod)
    {
        this.solver = solver;
        this.tolerancePeriod = tolerancePeriod ?? TimeSpan.FromMinutes(1);
    }
    
    public async Task<ChallengeResult> Solve(Uri uri)
    {
        var origin = uri.GetLeftPart(UriPartial.Authority);
        return await challengeResults.AddOrUpdate(origin,
            (o) => SolveChallenge(o, uri),
            async (o, oldTask) => await UpdateChallenge(o, uri, await oldTask));
    }

    private async Task<ChallengeResult> SolveChallenge(string origin, Uri uri)
    {
        return await this.solver.Solve(uri);
    }

    private Task<ChallengeResult> UpdateChallenge(string origin, Uri uri, ChallengeResult oldResult)
    {
        if (GetCurrentTime() + tolerancePeriod < oldResult.ExpiryTime)
        {
            return SolveChallenge(origin, uri);
        }
        else
        {
            return Task.FromResult(oldResult);
        }
    }

    private DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}