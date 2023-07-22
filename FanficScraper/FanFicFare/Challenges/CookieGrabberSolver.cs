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
    private readonly ILogger<CookieGrabberSolver> logger;

    public CookieGrabberSolver(HttpClient client, ILogger<CookieGrabberSolver> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<ChallengeSolution> Solve(Uri uri)
    {
        var response = await this.client.GetAsync($"/ClearForWebsite?url={HttpUtility.UrlEncode(uri.ToString())}");
        
        var result = await JsonSerializer.DeserializeAsync<CloudflareClearanceResult>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? throw new InvalidDataException();

        var expiryTimeInSeconds = result.Cookies
            .FirstOrDefault(cookie => cookie.Name == "cf_clearance")?.Expires;
        var expiryTime = expiryTimeInSeconds == null
            ? DateTime.UtcNow.AddDays(14)
            : DateTimeOffset.FromUnixTimeSeconds((long)expiryTimeInSeconds).UtcDateTime;

        var solution = new ChallengeSolution(
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
        
        logger.LogInformation("Found solution {0}", solution);
        
        return solution;
    }

    public void Invalidate(ChallengeSolution solved)
    {
        
    }
}