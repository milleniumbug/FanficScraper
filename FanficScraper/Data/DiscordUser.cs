namespace FanficScraper.Data;

public class DiscordUser
{
    public ulong Id { get; set; }
    
    public bool CanRead { get; set; }
    
    public bool CanMarkStoriesAsArchived { get; set; }
    
    public bool CanAddStories { get; set; }
}