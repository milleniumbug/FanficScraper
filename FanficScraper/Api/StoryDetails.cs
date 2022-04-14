namespace FanficScraper.Api;

public class StoryDetails
{
    public string Author { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public bool IsComplete { get; set; }
    public DateTime StoryUpdated { get; set; }
}