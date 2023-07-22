using System.Text.RegularExpressions;
using BrowserCookieGrabberService.Database.FirefoxCookies;
using Common;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BrowserCookieGrabberService;

public class FirefoxCookieGrabber
{
    private readonly ILogger<FirefoxCookieGrabber> logger;
    private readonly IDbContextFactory<FirefoxCookiesContext> contextFactory;
    private readonly IReadOnlyCollection<string> allowedSitesList;
    private readonly List<Regex> cookieNamePatterns;

    public FirefoxCookieGrabber(
        ILogger<FirefoxCookieGrabber> logger,
        IDbContextFactory<FirefoxCookiesContext> contextFactory,
        IEnumerable<string> allowedSitesList,
        IEnumerable<string> cookieNamePatterns)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
        this.allowedSitesList = allowedSitesList
            .Select(site => new Uri(site).Authority)
            .ToList();
        this.cookieNamePatterns = cookieNamePatterns
            .Select(pattern =>
            {
                var p = Regex.Escape(pattern);
                p = p.Replace("\\*", ".*");
                return new Regex(p);
            })
            .ToList();
    }

    public async Task<IEnumerable<CookieDto>> GrabCookiesForSite(string url)
    {
        var origin = new Uri(url).Authority;

        if (!allowedSitesList.Contains(origin))
        {
            this.logger.LogInformation("Site '{0} not on the allowed site list", origin);
            return Enumerable.Empty<CookieDto>();
        }

        string dottedSource = Regex.Match("." + origin, @".*\.([^.]+\.[^.]+)", RegexOptions.Singleline).Groups[1].Value;
        
        await using var cookieContext = await this.contextFactory.CreateDbContextAsync();
        var dbCookies = await cookieContext.MozCookies
            .Where(cookie => cookie.Host.EndsWith(dottedSource) && origin.Contains(cookie.Host))
            .ToListAsync();
        return dbCookies
            .Select(cookie => new CookieDto()
            {
                Name = cookie.Name!,
                Expires = (double)cookie.Expiry,
                Domain = cookie.Host!,
                HttpOnly = cookie.IsHttpOnly != 0,
                Path = cookie.Path!,
                Secure = cookie.IsSecure != 0,
                Value = cookie.Value!,
                //SameSite = cookie.SameSite != 0,
                //Session = cookie
            })
            //.Where(cookie => cookie.Expires)
            .ToList();
    }
}