namespace FanficScraper.FanFicFare;

public interface IFanFicFare
{
    Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false, bool force = false);
}