using System.Text;
using FanficScraper.Configurations;
using FanficScraper.FanFicFare;
using FanficScraper.FanFicFare.Challenges;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace TestProject1;

public class Sandbox
{
    private readonly ITestOutputHelper testOutputHelper;

    public Sandbox(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Sample()
    {
        var cacheSolver = new FilteringChallengeSolver(
            FilteringChallengeSolver.InclusionType.SolveAllChallengesExceptOnTheList,
            new []{ "https://www.tgstorytime.com" },
            new CachingChallengeSolver(
                new FlareSolverr(new HttpClient()
                    {
                        BaseAddress = new Uri("http://localhost:8191"),
                        Timeout = TimeSpan.FromMinutes(2)
                    }),
                    TimeSpan.FromMinutes(5),
                NullLogger<CachingChallengeSolver>.Instance));

        var v1 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com"));
        ;
        var v2 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/443703/the-harem-protagonist-was-turned-into-a-girl-and-doesnt-want-to-change-back/"));
        ;
        var v3 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/194683/acceptance-of-the-self/"));
        ;
        var v4 = await cacheSolver.Solve(new Uri("https://www.tgstorytime.com/viewstory.php?sid=4160"));
        ;
        PrintOutCookies(v2);
        PrintOutCookies(v4);
    }

    private void PrintOutCookies(ChallengeSolution solution)
    {
        using var memory = new MemoryStream();
        FanFicFare.WriteCookiesInMozillaFormat(memory, solution);

        var s = Encoding.UTF8.GetString(memory.ToArray());
        testOutputHelper.WriteLine(s);
    }
}