namespace FanficScraper.FanFicFare;

public class CompositeFanFicFare : IFanFicFare
{
    private readonly ILogger<CompositeFanFicFare> logger;
    private readonly List<IFanFicFare> clients;

    public CompositeFanFicFare(
        IEnumerable<IFanFicFare> clients,
        ILogger<CompositeFanFicFare> logger)
    {
        this.logger = logger;
        this.clients = clients.ToList();
    }

    public async Task<FanFicStoryDetails> Run(Uri storyUrl, bool metadataOnly = false, bool force = false)
    {
        Exception ex = new InvalidOperationException();
        foreach (var client in clients)
        {
            try
            {
                return await client.Run(storyUrl, metadataOnly, force);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Failed to run a subscraper");
                ex = e;
            }
        }

        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex;
    }
}