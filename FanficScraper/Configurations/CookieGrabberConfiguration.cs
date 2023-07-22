namespace FanficScraper.Configurations;

public class CookieGrabberConfiguration
{
    public bool EnableCookieGrabber { get; set; } = false;

    public string Address { get; set; } = "http://localhost:8191";

    public int TimeoutInMilliseconds { get; set; } = 60000;

    public string? UrlsToSolve { get; set; } = null;
}