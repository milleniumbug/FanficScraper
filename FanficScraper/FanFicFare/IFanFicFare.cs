namespace FanficScraper.FanFicFare;

public interface IFanFicFare
{
    Task<FanFicStoryDetails> Run(Uri storyUrl, bool metadataOnly = false, bool force = false);
}