namespace FanficScraper.FanFicFare.Challenges;

public interface IChallengeSolver
{
    public Task<ChallengeResult> Solve(Uri uri);
}