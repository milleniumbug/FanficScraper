using System.Text.Json;
using Common;
using Common.Api;
using FanficScraper.Configurations;
using FanficScraper.Services;
using Microsoft.Extensions.Options;
using ScribbleHubFeed;

namespace FanficScraper.FanFicFare;

public class ScribbleHubFeedUpdaterService : SimpleTimerHostedService
{
    private readonly ILogger<ScribbleHubFeedUpdaterService> logger;

    public ScribbleHubFeedUpdaterService(
        ILogger<ScribbleHubFeedUpdaterService> logger,
        IServiceScopeFactory scopeFactory) : base(logger, scopeFactory)
    {
        this.logger = logger;
    }

    protected override async Task<TimeSpan?> DoWork(IServiceProvider serviceProvider)
    {
        var scribbleHubFeed = serviceProvider.GetRequiredService<ScribbleHubFeed.ScribbleHubFeed>();
        var configuration = serviceProvider.GetRequiredService<IOptions<DataConfiguration>>().Value;

        var seriesFinderSettings = JsonSerializer.Deserialize<List<SeriesFinderSettings>>(configuration.ScribbleHub.QueryJson)
            ?? throw new JsonException();

        await MainRun(scribbleHubFeed, configuration.ScribbleHub, seriesFinderSettings);
        
        return TimeSpan.FromHours(6);
    }
    
    private async Task MainRun(
        ScribbleHubFeed.ScribbleHubFeed scribbleHubFeed,
        ScribbleHubFeedConfiguration configuration,
        IReadOnlyList<SeriesFinderSettings> seriesFinderSettings)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(configuration.FanFicScraperInstanceUrl);
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        
        var fanficscraper = new FanFicScraperClient(
            httpClient);
        
        var visitedStories = new HashSet<Uri>();
        foreach (var seriesFinderSetting in seriesFinderSettings)
        {
            await ScrapeFeed(
                logger,
                scribbleHubFeed,
                configuration,
                seriesFinderSetting,
                visitedStories,
                fanficscraper);
        }
    }

    public static async Task ScrapeFeed(ILogger<ScribbleHubFeedUpdaterService> logger,
        ScribbleHubFeed.ScribbleHubFeed scribbleHubFeed,
        ScribbleHubFeedConfiguration configuration,
        SeriesFinderSettings seriesFinderSetting,
        ISet<Uri> visitedStories,
        FanFicScraperClient fanficscraper)
    {
        while (true)
        {
            try
            {
                var results = scribbleHubFeed.SeriesFinder(
                    seriesFinderSetting);

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

                        logger.LogInformation("Handling the story under url '{Url}' and title '{Title}'", story.Uri,
                            story.Title);
                        var storyByNameResult = await fanficscraper.FindStories(story.Title);
                        if (storyByNameResult.Results.Any(s => s.Url == story.Uri.ToString()))
                        {
                            logger.LogInformation("Already added {Title}", story.Title);
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
                            logger.LogInformation("New story added {Title}", story.Title);
                            var downloadJob = await fanficscraper.AddStoryAsync(new AddStoryAsyncCommand()
                            {
                                Force = false,
                                Passphrase = configuration.FanFicScraperInstancePassword,
                                Url = story.Uri.ToString()
                            });
                            jobs.Add(downloadJob);
                            alreadyAddedStoriesCount = 0;
                        }
                        else
                        {
                            logger.LogInformation("Already added {Title}", story.Title);
                            alreadyAddedStoriesCount++;
                        }

                        visitedStories.Add(story.Uri);
                    }

                    logger.LogInformation("End of page");
                    if (alreadyAddedStoriesCount > configuration.AlreadyAddedStoriesMaxCount)
                    {
                        break;
                    }
                }

                foreach (var job in jobs)
                {
                    var result = await fanficscraper.AwaitAdd(job);
                    logger.LogInformation("{Url} - {Status} {StoryId}", result.Url, result.Status, result.StoryId);
                }

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error encountered");
            }
        }
    }
}