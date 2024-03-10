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
var scrapeCache = new LocalDirectoryCache("<TO FILL>");

var fanficscraper = new FanFicScraperClient(
    new HttpClient()
    {
        BaseAddress = fanficscraperAddress
    });

var loggerBuilder = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var challengeSolver = new CookieGrabberSolver(
    new HttpClient()
    {
        BaseAddress = cookieGrabberAddress,
        Timeout = TimeSpan.FromMinutes(5),
    },
    loggerBuilder.CreateLogger<CookieGrabberSolver>());

var (solution, _) = await challengeSolver.Solve(new Uri(scribbleHubAddress));

var cookieContainer = new CookieContainer();
using var handler = new HttpClientHandler()
{
    CookieContainer = cookieContainer
};
var client = new HttpClient(handler)
{
    BaseAddress = new Uri(scribbleHubAddress)
};

if (solution.Cookies != null)
{
    foreach (var cookie in solution.Cookies)
    {
        cookieContainer.Add(new Uri(scribbleHubAddress), cookie);
    }
}

if (solution.UserAgent != null)
{
    client.DefaultRequestHeaders.Add("User-Agent", solution.UserAgent);
}

var scraper = new CachingScraper(
    client,
    scrapeCache);
    
var scribbleHubFeed = new ScribbleHubFeed.ScribbleHubFeed(scraper);

var results = scribbleHubFeed.ByTag(
    ScribbleHubTags.Transgender,
    SortCriteria.DateAdded,
    SortOrder.Descending,
    StoryStatus.Ongoing);

var jobs = new List<AddStoryAsyncCommandResponse>();
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
}

foreach (var job in jobs)
{
    var result = await fanficscraper.AwaitAdd(job);
    Console.WriteLine($"{result.Url} - {result.Status} {result.StoryId}");
}