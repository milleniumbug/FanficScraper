using System.Text.Json;
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
        var fanFicUpdater = serviceProvider.GetRequiredService<FanFicUpdater>();
        var storyBrowser = serviceProvider.GetRequiredService<StoryBrowser>();
        var scribbleHubFeed = serviceProvider.GetRequiredService<ScribbleHubFeed.ScribbleHubFeed>();
        var configuration = serviceProvider.GetRequiredService<IOptions<DataConfiguration>>().Value;

        var seriesFinderSettings = JsonSerializer.Deserialize<SeriesFinderSettings>(configuration.ScribbleHub.QueryJson)
            ?? throw new JsonException();
        
        logger.LogInformation("Attempting to find more series on ScribbleHub");

        var feedPages = scribbleHubFeed.SeriesFinder(seriesFinderSettings);
        
        int page = 1;
        await foreach (var feedPage in feedPages)
        {
            logger.LogInformation("Starting scanning page number {PageNumber}", page);
            foreach (var story in feedPage)
            {
                var result = await storyBrowser.FindByUrl(story.Uri);
                if (result == null)
                {
                    logger.LogInformation("Found new story '{StoryName}' under URL {StoryUrl}, attempting to grab it", story.Title, story.Uri);
                    await fanFicUpdater.UpdateStory(story.Uri, force: false);
                }
                else
                {
                    logger.LogInformation("Story '{StoryName}' is already added", story.Title);
                }
            }
        }
        
        return TimeSpan.FromHours(72);
    }
}