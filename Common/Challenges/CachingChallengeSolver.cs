using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Common.Challenges;

public class CachingChallengeSolver : IChallengeSolver
{
    private readonly IChallengeSolver solver;
    private readonly ILogger<CachingChallengeSolver> logger;
    private readonly ConcurrentDictionary<string, Task<ChallengeSolution>> challengeResults = new();
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
    
    public async Task<ChallengeSolution> Solve(Uri uri)
    {
        var origin = uri.GetOrigin();
        return await challengeResults.AddOrUpdate(origin,
            (o) =>
            {
                this.logger.LogInformation("First time encountering {Origin}, calling into the challenge solver", o);
                return SolveChallenge(o, uri);
            },
            async (o, oldTask) => await UpdateChallenge(o, uri, await oldTask));
    }

    public void Invalidate(ChallengeSolution solved)
    {
        this.challengeResults.TryRemove(solved.Origin.GetOrigin(), out _);
        this.solver.Invalidate(solved);
    }

    private async Task<ChallengeSolution> SolveChallenge(string origin, Uri uri)
    {
        return await this.solver.Solve(uri);
    }

    private Task<ChallengeSolution> UpdateChallenge(string origin, Uri uri, ChallengeSolution oldSolution)
    {
        var currentTime = GetCurrentTime();
        if (currentTime > oldSolution.ExpiryTime + tolerancePeriod)
        {
            this.logger.LogInformation("Calling the solver with {Uri} for {Origin}, as the old solution's {Solution} expiration time is older than {CurrentTime}",
                uri, origin, oldSolution, currentTime);
            return SolveChallenge(origin, uri);
        }
        else
        {
            this.logger.LogInformation("Reusing the old solution {Solution}", oldSolution);
            return Task.FromResult(oldSolution);
        }
    }

    private DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}