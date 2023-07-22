using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace BrowserCookieGrabberService.Controllers;

[ApiController]
public class CloudflareClearanceController : ControllerBase
{
    [HttpGet("ClearForWebsite")]
    public async Task<CloudflareClearanceResult> ClearForWebsite(
        [FromQuery] string url,
        [FromServices] FirefoxCookieGrabber cookieGrabber,
        [FromServices] UserAgentGrabber userAgentGrabber)
    {
        var cookies = await cookieGrabber.GrabCookiesForSite(url);
        return new CloudflareClearanceResult()
        {
            UserAgent = userAgentGrabber.UserAgent,
            Cookies = cookies.ToList()
        };
    }

    [HttpGet("SetUserAgent")]
    public void SetUserAgent(
        [FromServices] UserAgentGrabber userAgentGrabber)
    {
        userAgentGrabber.SetUserAgent(HttpContext.Request.Headers[HeaderNames.UserAgent]);
    }
}