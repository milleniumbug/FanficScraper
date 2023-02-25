using System.Text;
using FanficScraper.Configurations;
using FanficScraper.FanFicFare;
using FanficScraper.FanFicFare.Challenges;
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
        var cacheSolver = new CachingChallengeSolver(
            new FlareSolverr(new HttpClient()
                {
                    BaseAddress = new Uri("http://localhost:8191")
                },
                new FlareSolverrConfiguration()
                {
                    TimeoutInMilliseconds = 120_000
                }),
            TimeSpan.FromMinutes(5));

        var v1 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com"));
        ;
        var v2 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/443703/the-harem-protagonist-was-turned-into-a-girl-and-doesnt-want-to-change-back/"));
        ;
        var v3 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/194683/acceptance-of-the-self/"));
        ;
        using var memory = new MemoryStream();
        FanFicFare.WriteCookiesInMozillaFormat(memory, v2);

        var s = Encoding.UTF8.GetString(memory.ToArray());
        testOutputHelper.WriteLine(s);
    }
}