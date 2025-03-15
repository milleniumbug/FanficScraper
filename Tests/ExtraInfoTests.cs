using Common.Caching;
using Common.Challenges;
using Common.Scraping;
using FanficScraper.FanFicFare;
using Meziantou.Extensions.Logging.Xunit;
using Xunit.Abstractions;

namespace TestProject1;

public class ExtraInfoTests
{
    private readonly CachingScraper cachingScraper;

    public ExtraInfoTests(ITestOutputHelper testOutputHelper)
    {
        var solver = new CookieGrabberSolver(
            new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:12000"),
                Timeout = TimeSpan.FromMinutes(2),
            },
            XUnitLogger.CreateLogger<CookieGrabberSolver>(testOutputHelper));

        var httpClient = new HttpClient(new ChallengeSolverHandler(solver));
        
        this.cachingScraper = new CachingScraper(
            httpClient,
            new NullCache<string, string>(),
            XUnitLogger.CreateLogger<CachingScraper>(testOutputHelper));
    }

    [Fact]
    public async Task Test()
    {
        var extraInfo = new ExtraInfo(
            new Uri("https://www.scribblehub.com/series/253998/faraway-survivor/"),
            cachingScraper);

        var license = await extraInfo.GetLicense();
        Assert.Equal(
            actual: license,
            expected: "All Rights Reserved");
    }
}