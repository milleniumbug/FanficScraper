using System.Globalization;
using FanficScraper.Configurations;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicAutoUpdaterService : IHostedService, IDisposable
{
    private readonly ILogger<FanFicAutoUpdaterService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private Timer? timer;

    public FanFicAutoUpdaterService(
        ILogger<FanFicAutoUpdaterService> logger,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Automatic Update Service running");

        timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            using var scope = this.scopeFactory.CreateScope();
            var fanFicUpdater = scope.ServiceProvider.GetRequiredService<FanFicUpdater>();
            var configuration = scope.ServiceProvider.GetRequiredService<IOptions<DataConfiguration>>().Value;

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
            
            timer?.Change(nextUpdate, Timeout.InfiniteTimeSpan);
            logger.LogInformation("Next auto-update on '{0}'", (currentDate + nextUpdate).ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
            timer?.Change(TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Automatic Update Service is stopping");

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}