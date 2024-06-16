using Common;
using Common.Models;
using FanficScraper.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace BrowserCookieGrabberService.Controllers;

[ApiController]
public class CloudflareClearanceController : ControllerBase
{
    private readonly ILogger<CloudflareClearanceController> logger;

    public CloudflareClearanceController(ILogger<CloudflareClearanceController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("ClearForWebsite")]
    public async Task<CloudflareClearanceResult> ClearForWebsite(
        [FromQuery] string url,
        [FromServices] FirefoxCookieGrabber cookieGrabber,
        [FromServices] UserAgentGrabber userAgentGrabber)
    {
        var cookies = (await cookieGrabber.GrabCookiesForSite(url)).ToList();
        logger.LogInformation("Grabbed cookies for site {0}. Cookie names: {1}", url, new ToStringableReadOnlyList<string>(cookies.Select(cookie => cookie.Name).ToList()));
        return new CloudflareClearanceResult()
        {
            UserAgent = userAgentGrabber.UserAgent,
            Cookies = cookies,
            CookiesMozillaFormat = CookieUtils.StringFromCookies(cookies)
        };
    }

    [HttpGet("SetUserAgent")]
    public string SetUserAgent(
        [FromServices] UserAgentGrabber userAgentGrabber)
    {
        var ua = HttpContext.Request.Headers[HeaderNames.UserAgent];
        userAgentGrabber.SetUserAgent(ua);
        logger.LogInformation("User agent set to {0}.", ua);
        return "Success: You can close the tab.";
    }
}