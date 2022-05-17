namespace FanficScraper.Api;

public class AddStoryAsyncCommand
{
    public string Url { get; set; }
    
    public string Passphrase { get; set; }
    
    public bool? Force { get; set; }
}