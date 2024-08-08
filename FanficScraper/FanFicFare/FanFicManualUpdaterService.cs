using System.Globalization;
using FanficScraper.Data;
using FanficScraper.Services;

namespace FanficScraper.FanFicFare;

public class FanFicManualUpdaterService : SimpleTimerHostedService
{
    private readonly ILogger<FanFicManualUpdaterService> logger;
    private readonly Guid runnerId = Guid.NewGuid();

    public FanFicManualUpdaterService(
        ILogger<FanFicManualUpdaterService> logger,
        IServiceScopeFactory scopeFactory) : base(logger, scopeFactory)
    {
        this.logger = logger;
    }

    protected override async Task<TimeSpan?> DoWork(IServiceProvider serviceProvider)
    {
        var fanFicUpdater = serviceProvider.GetRequiredService<FanFicUpdater>();
        try
        {
            var story = await fanFicUpdater.UpdateNextScheduled(runnerId);
            if (story != null)
            {
                logger.LogInformation("Updated manually scheduled story: {Title} by {Author}", story.Title, story.Author);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
        }

        return TimeSpan.FromSeconds(15);
    }
}