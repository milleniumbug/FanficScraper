using System.Net;
using Common.Challenges;
using FanficScraper.FanFicFare;
using Meziantou.Extensions.Logging.Xunit;
using Xunit.Abstractions;

namespace TestProject1;

public class CloudflareBreak
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly IChallengeSolver cacheSolver;

    public CloudflareBreak(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        
        cacheSolver = new FilteringChallengeSolver(
            FilteringChallengeSolver.InclusionType.SolveNoChallengeExceptOnTheList,
            "https://www.fanfiction.net/;https://fanfiction.net/;https://www.scribblehub.com/;https://scribblehub.com/".Split(";"),
            new CookieGrabberSolver(
                new HttpClient()
                {
                    BaseAddress = new Uri("http://localhost:12000"),
                    Timeout = TimeSpan.FromMinutes(2),
                },
                XUnitLogger.CreateLogger<CookieGrabberSolver>(this.testOutputHelper)));
    }
        
    [Fact]
    public async Task GoToScribbleHub()
    {
        var httpClient = new HttpClient(new ChallengeSolverHandler(cacheSolver));

        var response = await httpClient.GetAsync("https://www.scribblehub.com/series-finder/?sf=1&tgi=1088&order=desc&sort=dateadded&pg=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Latest Series", content);
    }
    
    [Fact]
    public async Task FanFicFare()
    {
        var temp = Directory.CreateTempSubdirectory();
        var fanficfare = new FanFicFare(
            new FanFicFareSettings()
            {
                IsAdult = true,
                ChallengeSolver = cacheSolver,
                FanFicFareExecutablePath = "fanficfare",
                IncludeImages = false,
                TargetDirectory = temp.FullName,
            },
            XUnitLogger.CreateLogger<FanFicFare>(this.testOutputHelper));

        var details = await fanficfare.Run(new Uri("https://www.scribblehub.com/series/1171186/concealed-no-more/"), metadataOnly: true);
    }
}