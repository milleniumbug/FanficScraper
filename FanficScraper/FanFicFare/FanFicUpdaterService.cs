namespace FanficScraper.FanFicFare;

public class FanFicUpdaterService : IHostedService, IDisposable
{
    private readonly ILogger<FanFicUpdaterService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private int executionCount = 0;
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

        timer = new Timer(DoWork, null, TimeSpan.Zero, 
            TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            using var scope = this.scopeFactory.CreateScope();
            var fanFicUpdater = scope.ServiceProvider.GetRequiredService<FanFicUpdater>();
            logger.LogInformation("Updating a story");
            var story = await fanFicUpdater.UpdateOldest(TimeSpan.FromDays(1));
            if (story != null)
            {
                logger.LogInformation("Updated {0} by {1}", story.Title, story.Author);
            }
            else
            {
                logger.LogInformation("No story needs update");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failure while updating a chapter");
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