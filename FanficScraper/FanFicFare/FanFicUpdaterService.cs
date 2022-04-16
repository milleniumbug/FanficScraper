using System.Globalization;

namespace FanficScraper.FanFicFare;

public class FanFicUpdaterService : IHostedService, IDisposable
{
    private readonly ILogger<FanFicUpdaterService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private Timer? timer;

    public FanFicUpdaterService(
        ILogger<FanFicUpdaterService> logger,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service running");

        timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            var minimalTime = TimeSpan.FromSeconds(30);
            using var scope = this.scopeFactory.CreateScope();
            var fanFicUpdater = scope.ServiceProvider.GetRequiredService<FanFicUpdater>();
            logger.LogInformation("Updating a story");
            var (story, nextUpdateIn) = await fanFicUpdater.UpdateOldest(TimeSpan.FromDays(1));
            if (story != null)
            {
                logger.LogInformation("Updated {0} by {1}", story.Title, story.Author);
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
            
            timer?.Change(nextUpdate, Timeout.InfiniteTimeSpan);
            logger.LogInformation("Next update on '{0}'", (currentDate + nextUpdate).ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
            timer?.Change(TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service is stopping");

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}