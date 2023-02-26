using System.Collections.Concurrent;

namespace FanficScraper.FanFicFare.Challenges;

public class CachingChallengeSolver : IChallengeSolver
{
    private readonly IChallengeSolver solver;
    private readonly ILogger<CachingChallengeSolver> logger;
    private readonly ConcurrentDictionary<string, Task<ChallengeResult>> challengeResults = new();
    private readonly TimeSpan tolerancePeriod;

    public CachingChallengeSolver(
        IChallengeSolver solver,
        TimeSpan? tolerancePeriod,
        ILogger<CachingChallengeSolver> logger)
    {
        this.solver = solver;
        this.logger = logger;
        this.tolerancePeriod = tolerancePeriod ?? TimeSpan.FromMinutes(1);
    }
    
    public async Task<ChallengeResult> Solve(Uri uri)
    {
        var origin = uri.GetLeftPart(UriPartial.Authority);
        return await challengeResults.AddOrUpdate(origin,
            (o) =>
            {
                this.logger.LogInformation("First time encountering {Origin}, calling into the challenge solver", o);
                return SolveChallenge(o, uri);
            },
            async (o, oldTask) => await UpdateChallenge(o, uri, await oldTask));
    }

    private async Task<ChallengeResult> SolveChallenge(string origin, Uri uri)
    {
        return await this.solver.Solve(uri);
    }

    private Task<ChallengeResult> UpdateChallenge(string origin, Uri uri, ChallengeResult oldResult)
    {
        var currentTime = GetCurrentTime();
        if (currentTime > oldResult.ExpiryTime + tolerancePeriod)
        {
            this.logger.LogInformation("Calling the solver with {Uri} for {Origin}, as the old solution's {Solution} expiration time is older than {CurrentTime}",
                uri, origin, oldResult, currentTime);
            return SolveChallenge(origin, uri);
        }
        else
        {
            this.logger.LogInformation("Reusing the old solution {Solution}", oldResult);
            return Task.FromResult(oldResult);
        }
    }

    private DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}