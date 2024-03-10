namespace Common.Api;

public class AddStoryCommand
{
    public string Url { get; set; }
    
    public string Passphrase { get; set; }
    
    public bool? Force { get; set; }
}