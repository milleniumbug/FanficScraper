using Common.Caching;
using Common.Challenges;
using Common.Scraping;
using Meziantou.Extensions.Logging.Xunit;
using ScribbleHubFeed;
using Xunit.Abstractions;

namespace TestProject1;

public class ScribbleHub
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly CookieGrabberSolver solver;

    public ScribbleHub(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;

        this.solver = new CookieGrabberSolver(
            new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:12000"),
                Timeout = TimeSpan.FromMinutes(2),
            },
            XUnitLogger.CreateLogger<CookieGrabberSolver>(this.testOutputHelper));
    }

    [Fact]
    public async Task Feed()
    {
        var s = new SeriesFinderSettings()
        {
            Status = StoryStatus.All,
            SortDirection = SortOrder.Descending,
            SortBy = SortCriteria.DateAdded,
            TagInclusion = Alternative.And,
            IncludedTags = new []{ Tags.Parse("Transgender") }
        };

        var httpClient = new HttpClient(new ChallengeSolverHandler(this.solver));

        var feed = new ScribbleHubFeed.ScribbleHubFeed(
            new CachingScraper(
                httpClient,
                new NullCache<string, string>(),
                XUnitLogger.CreateLogger<CachingScraper>(this.testOutputHelper)));

        var series = feed.SeriesFinder(s);
        await foreach (var story in series.Take(2).SelectMany(page => page.ToAsyncEnumerable()))
        {
            this.testOutputHelper.WriteLine($"{story.Title}: {story.Uri}");
        }
    }
}