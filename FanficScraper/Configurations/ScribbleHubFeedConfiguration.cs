namespace FanficScraper.Configurations;

public class ScribbleHubFeedConfiguration
{
    public bool EnableScribbleHubFeed { get; set; } = false;
    
    public int TimeoutInMilliseconds { get; set; } = 15000;

    public string QueryJson { get; set; }
}