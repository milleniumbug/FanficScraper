using System.Globalization;
using FanficScraper.Configurations;
using FanficScraper.Services;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicAutoUpdaterService : SimpleTimerHostedService
{
    private readonly ILogger<FanFicAutoUpdaterService> logger;

    public FanFicAutoUpdaterService(
        ILogger<FanFicAutoUpdaterService> logger,
        IServiceScopeFactory scopeFactory) : base(logger, scopeFactory)
    {
        this.logger = logger;
    }

    protected override async Task<TimeSpan?> DoWork(IServiceProvider serviceProvider)
    {
        try
        {
            var fanFicUpdater = serviceProvider.GetRequiredService<FanFicUpdater>();
            var configuration = serviceProvider.GetRequiredService<IOptions<DataConfiguration>>().Value;

            var minimalTimeInSeconds = Random.Shared.Next(
                minValue: configuration.MinimumUpdateDistanceInSecondsLowerBound,
                maxValue: configuration.MinimumUpdateDistanceInSecondsUpperBound);
            var minimalTime = TimeSpan.FromSeconds(minimalTimeInSeconds);
            
            logger.LogInformation("Updating a story");
            var (story, nextUpdateIn) = await fanFicUpdater.UpdateOldest(TimeSpan.FromDays(1));
            if (story != null)
            {
                logger.LogInformation("Autoupdated {0} by {1}", story.Title, story.Author);
            }
            else
            {
                logger.LogInformation("No story needs update");
            }

            var currentDate = DateTime.UtcNow;
            var nextUpdate = nextUpdateIn - currentDate;
            if (nextUpdate < minimalTime)
            {
                nextUpdate = minimalTime;
            }
            
            logger.LogInformation("Next auto-update on '{0}'", (currentDate + nextUpdate).ToString(CultureInfo.InvariantCulture));
            return nextUpdate;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
            return TimeSpan.FromSeconds(30);
        }
    }
}