using System.Globalization;
using FanficScraper.Data;

namespace FanficScraper.FanFicFare;

public class FanFicManualUpdaterService : IHostedService, IDisposable
{
    private readonly ILogger<FanFicManualUpdaterService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private Timer? timer;
    private Guid runnerId = Guid.NewGuid();

    public FanFicManualUpdaterService(
        ILogger<FanFicManualUpdaterService> logger,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Manual schedule service running");

        timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        using var scope = this.scopeFactory.CreateScope();
        var fanFicUpdater = scope.ServiceProvider.GetRequiredService<FanFicUpdater>();
        try
        {
            var story = await fanFicUpdater.UpdateNextScheduled(runnerId);
            if (story != null)
            {
                logger.LogInformation("Updated manually scheduled story: {0} by {1}", story.Title, story.Author);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
        }
        timer?.Change(TimeSpan.FromSeconds(15), Timeout.InfiniteTimeSpan);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Manual schedule service is stopping");

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}