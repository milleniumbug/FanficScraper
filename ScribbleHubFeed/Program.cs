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
var fanficscraperAddress = new Uri("TO FILL");
var cookieGrabberAddress = new Uri("TO FILL");
var scrapeCache = new NullCache<string, string>();

var fanficscraper = new FanFicScraperClient(
    new HttpClient()
    {
        BaseAddress = fanficscraperAddress,
        Timeout = Timeout.InfiniteTimeSpan,
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
    BaseAddress = new Uri(scribbleHubAddress),
    Timeout = Timeout.InfiniteTimeSpan,
};

var scraper = new CachingScraper(
    client,
    scrapeCache);
    
var scribbleHubFeed = new ScribbleHubFeed.ScribbleHubFeed(scraper);

await MainRun();

async Task MainRun()
{
    var visitedStories = new HashSet<Uri>();
    while (true)
    {
        try
        {
            var results = scribbleHubFeed.SeriesFinder(
                new SeriesFinderSettings()
                {
                    Status = StoryStatus.All,
                    SortDirection = SortOrder.Descending,
                    SortBy = SortCriteria.DateAdded,
                    IncludedTags = new[] { Tags.Parse("Transgender") }
                });

            var jobs = new List<AddStoryAsyncCommandResponse>();
            int alreadyAddedStoriesCount = 0;
            await foreach (var page in results)
            {
                foreach (var story in page)
                {
                    if (visitedStories.Contains(story.Uri))
                    {
                        continue;
                    }
                    
                    Console.WriteLine(story);
                    var storyByNameResult = await fanficscraper.FindStories(story.Title);
                    if (storyByNameResult.Results.Any(s => s.Url == story.Uri.ToString()))
                    {
                        Console.WriteLine($"already added {story.Title}");
                        alreadyAddedStoriesCount++;
                        continue;
                    }

                    var metadata = await fanficscraper.GetMetadata(new GetMetadataQuery()
                    {
                        Url = story.Uri.ToString()
                    });
                    var storyResult = await fanficscraper.GetStoryById(metadata.Id);
                    if (storyResult == null)
                    {
                        Console.WriteLine($"new story: {story.Title}");
                        var downloadJob = await fanficscraper.AddStoryAsync(new AddStoryAsyncCommand()
                        {
                            Force = false,
                            Passphrase = passphrase,
                            Url = story.Uri.ToString()
                        });
                        jobs.Add(downloadJob);
                        alreadyAddedStoriesCount = 0;
                    }
                    else
                    {
                        Console.WriteLine($"already added {story.Title}");
                        alreadyAddedStoriesCount++;
                    }

                    visitedStories.Add(story.Uri);
                }

                Console.WriteLine("end of page");
                if (alreadyAddedStoriesCount > alreadyAddedStoriesMaxCount)
                {
                    break;
                }
            }

            foreach (var job in jobs)
            {
                var result = await fanficscraper.AwaitAdd(job);
                Console.WriteLine($"{result.Url} - {result.Status} {result.StoryId}");
            }

            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}