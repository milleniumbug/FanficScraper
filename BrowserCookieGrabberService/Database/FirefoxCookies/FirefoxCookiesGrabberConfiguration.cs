namespace BrowserCookieGrabberService.Database.FirefoxCookies;

public class FirefoxCookiesGrabberConfiguration
{
    public string PathToProfile { get; set; }
    
    public string SitesAllowed { get; set; }
    
    public string CookieNamePattern { get; set; }
}