namespace FanficScraper.Data;

public class Story
{
    public int Id { get; set; }
    
    public string StoryName { get; set; }
    
    public string StoryUrl { get; set; }
    
    public string AuthorName { get; set; }
    
    public string Website { get; set; }
    
    public DateTime LastUpdated { get; set; }
    
    public bool IsComplete { get; set; }
    
    public DateTime StoryUpdated { get; set; }
    
    public string FileName { get; set; }
}