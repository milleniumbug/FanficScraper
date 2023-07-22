using System.Net;
using System.Text.Json;
using System.Web;
using Common;
using Common.Models;
using FanficScraper.Utils;

namespace FanficScraper.FanFicFare.Challenges;

public class CookieGrabberSolver : IChallengeSolver
{
    private readonly HttpClient client;

    public CookieGrabberSolver(HttpClient client)
    {
        this.client = client;
    }

    public async Task<ChallengeSolution> Solve(Uri uri)
    {
        var response = await this.client.GetAsync($"/ClearForWebsite?url={HttpUtility.UrlEncode(uri.ToString())}");
        
        var result = await JsonSerializer.DeserializeAsync<CloudflareClearanceResult>(
            await response.Content.ReadAsStreamAsync()) ?? throw new InvalidDataException();

        var expiryTimeInSeconds = result.Cookies
            .FirstOrDefault(cookie => cookie.Name == "cf_clearance")?.Expires;
        var expiryTime = expiryTimeInSeconds == null
            ? DateTime.UtcNow.AddDays(14)
            : DateTimeOffset.FromUnixTimeSeconds((long)expiryTimeInSeconds).UtcDateTime;

        return new ChallengeSolution(
            UserAgent: result.UserAgent,
            Cookies: new ToStringableReadOnlyList<Cookie>(
                result.Cookies
                    .Select(cookie => new Cookie()
                    {
                        Name = cookie.Name,
                        Value = cookie.Value,
                        Expires = DateTimeOffset.FromUnixTimeSeconds((long)cookie.Expires).UtcDateTime,
                        HttpOnly = cookie.HttpOnly,
                        Secure = cookie.Secure,
                        Domain = cookie.Domain,
                        Path = cookie.Path,
                    })
                    .ToList()),
            ExpiryTime: expiryTime,
            Origin: new Uri(uri.GetOrigin()));
    }

    public void Invalidate(ChallengeSolution solved)
    {
        
    }
}