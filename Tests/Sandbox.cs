using System.Text;
using Common;
using Common.Challenges;
using FanficScraper.Configurations;
using FanficScraper.FanFicFare;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using ScribbleHubFeed;
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
    public void Json()
    {
        var s = new SeriesFinderSettings()
        {
            Status = StoryStatus.All,
            SortDirection = SortOrder.Descending,
            SortBy = SortCriteria.DateAdded,
            TagInclusion = Alternative.And,
            IncludedTags = new []{ Tags.Parse("Transgender") }
        };

        var result = System.Text.Json.JsonSerializer.Serialize(s);
        var r = System.Text.Json.JsonSerializer.Serialize(result);
    }

    [Fact(Skip = "needs to be run explicitly")]
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
        CookieUtils.WriteCookiesInMozillaFormat(memory, solution.Cookies);

        var s = Encoding.UTF8.GetString(memory.ToArray());
        testOutputHelper.WriteLine(s);
    }
}