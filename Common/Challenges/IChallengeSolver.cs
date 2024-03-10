namespace Common.Challenges;

public interface IChallengeSolver
{
    public Task<ChallengeSolution> Solve(Uri uri);
    void Invalidate(ChallengeSolution solved);
}