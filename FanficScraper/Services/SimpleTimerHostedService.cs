namespace FanficScraper.Services;

public abstract class SimpleTimerHostedService : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly IServiceScopeFactory scopeFactory;
    private Timer? timer;
    
    public SimpleTimerHostedService(
        ILogger logger,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }
    
    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ServiceName} service running", this.GetType().Name);

        timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

        return Task.CompletedTask;
    }
    
    private async void DoWork(object? state)
    {
        using var scope = this.scopeFactory.CreateScope();
        var timespan = await DoWork(scope.ServiceProvider);
        if (timespan != null)
        {
            timer?.Change(timespan.Value, Timeout.InfiniteTimeSpan);
        }
    }

    protected abstract Task<TimeSpan?> DoWork(IServiceProvider serviceProvider);
    
    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ServiceName} is stopping", this.GetType().Name);

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}