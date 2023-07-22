namespace BrowserCookieGrabberService;

public class UserAgentGrabber
{
    public string? UserAgent { get; private set; }

    public void SetUserAgent(string ua)
    {
        if (UserAgent != null)
        {
            return;
        }

        UserAgent = ua;
    }
}