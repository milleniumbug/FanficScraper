using System.Net;
using Common;
using Common.Api;
using Common.Caching;
using Common.Challenges;
using Common.Scraping;
using Microsoft.Extensions.Logging;
using ScribbleHubFeed;

var scribbleHubAddress = "https://www.scribblehub.com";
var passphrase = "<TO FILL>";
var fanficscraperAddress = new Uri("<TO FILL>");
var cookieGrabberAddress = new Uri("<TO FILL>");
var scrapeCache = new NullCache<string, string>();

var fanficscraper = new FanFicScraperClient(
    new HttpClient()
    {
        BaseAddress = fanficscraperAddress
    });

var loggerBuilder = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var challengeSolver = new CachingChallengeSolver(
    new CookieGrabberSolver(
        new HttpClient()
        {
            BaseAddress = cookieGrabberAddress,
            Timeout = TimeSpan.FromMinutes(5),
        },
        loggerBuilder.CreateLogger<CookieGrabberSolver>()),
    TimeSpan.FromMinutes(30),
    loggerBuilder.CreateLogger<CachingChallengeSolver>());

using var handler = new ChallengeSolverHandler(challengeSolver);
var client = new HttpClient(handler)
{
    BaseAddress = new Uri(scribbleHubAddress)
};

var scraper = new CachingScraper(
    client,
    scrapeCache);
    
var scribbleHubFeed = new ScribbleHubFeed.ScribbleHubFeed(scraper);

try
{
    var results = scribbleHubFeed.ByTag(
        ScribbleHubTags.Transgender,
        SortCriteria.DateAdded,
        SortOrder.Descending,
        StoryStatus.Ongoing);

    var jobs = new List<AddStoryAsyncCommandResponse>();
    int x = 0;
    await foreach (var page in results)
    {
        foreach (var story in page)
        {
            Console.WriteLine(story);
            var downloadJob = await fanficscraper.AddStoryAsync(new AddStoryAsyncCommand()
            {
                Force = false,
                Passphrase = passphrase,
                Url = story.Uri.ToString()
            });
            jobs.Add(downloadJob);
        }
        Console.WriteLine("end of page");
    }

    foreach (var job in jobs)
    {
        var result = await fanficscraper.AwaitAdd(job);
        Console.WriteLine($"{result.Url} - {result.Status} {result.StoryId}");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}