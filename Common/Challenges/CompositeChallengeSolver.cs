using Microsoft.Extensions.Logging;

namespace Common.Challenges;

public class CompositeChallengeSolver : IChallengeSolver
{
    private readonly IReadOnlyCollection<IChallengeSolver> solvers;
    private readonly ILogger<CompositeChallengeSolver> logger;

    public CompositeChallengeSolver(
        IEnumerable<IChallengeSolver> solvers,
        ILogger<CompositeChallengeSolver> logger)
    {
        this.solvers = solvers.ToList();
        this.logger = logger;
    }

    public async Task<ChallengeSolution> Solve(Uri uri)
    {
        Exception ex = new InvalidOperationException();
        foreach (var client in this.solvers)
        {
            try
            {
                return await client.Solve(uri);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Failed to run a the subsolver");
                ex = e;
            }
        }

        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex;
    }

    public void Invalidate(ChallengeSolution solved)
    {
        foreach (var solver in solvers)
        {
            solver.Invalidate(solved);
        }
    }
}