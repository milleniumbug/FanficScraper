namespace FanficScraper.Api;

public class GetStoryQueryResponse
{
    public string Author { get; init; }
    public string Url { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
    public DateTime StoryUpdated { get; set; }
}