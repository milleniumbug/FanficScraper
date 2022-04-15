namespace FanficScraper.Data;

public class User
{
    public string Id { get; set; }
    
    public string Login { get; set; }
    
    public string PasswordHash { get; set; }
    
    public DateTime CreationDate { get; set; }
    
    public bool IsActivated { get; set; }
}