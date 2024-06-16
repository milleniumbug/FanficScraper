namespace FanficScraper.Configurations;

public class ScribbleHubFeedConfiguration
{
    public bool EnableScribbleHubFeed { get; set; } = false;

    public string QueryJson { get; set; }
    
    public string FanFicScraperInstanceUrl { get; set; }
    
    public string FanFicScraperInstancePassword { get; set; }

    public string ScribbleHubAddress { get; set; } = "https://www.scribblehub.com";
}