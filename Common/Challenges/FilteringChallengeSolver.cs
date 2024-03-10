namespace Common.Challenges;

public class FilteringChallengeSolver : IChallengeSolver
{
    private readonly InclusionType inclusionType;
    private readonly HashSet<string> originUrls;
    private readonly IChallengeSolver solver;

    public enum InclusionType
    {
        SolveNoChallengeExceptOnTheList,
        SolveAllChallengesExceptOnTheList
    }

    public FilteringChallengeSolver(
        InclusionType inclusionType,
        IEnumerable<string> originUrlList,
        IChallengeSolver solver)
    {
        this.inclusionType = inclusionType switch
        {
            InclusionType.SolveNoChallengeExceptOnTheList => inclusionType,
            InclusionType.SolveAllChallengesExceptOnTheList => inclusionType,
            _ => throw new ArgumentOutOfRangeException(nameof(inclusionType), inclusionType, null)
        };
        this.originUrls = originUrlList.ToHashSet();
        this.solver = solver;
    }


    public async Task<ChallengeSolution> Solve(Uri uri)
    {
        var origin = uri.GetOrigin();
        bool isOnTheList = originUrls.Contains(origin);
        if (inclusionType == InclusionType.SolveNoChallengeExceptOnTheList && isOnTheList ||
            inclusionType == InclusionType.SolveAllChallengesExceptOnTheList && !isOnTheList)
        {
            return await this.solver.Solve(uri);
        }
        else
        {
            return ChallengeSolution.GetNeverExpiringNullSolution(new Uri(origin));
        }
    }

    public void Invalidate(ChallengeSolution solved)
    {
        this.solver.Invalidate(solved);
    }
}