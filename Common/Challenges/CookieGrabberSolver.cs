using System.Net;
using System.Text.Json;
using System.Web;
using Common.Models;
using FanficScraper.Utils;
using Microsoft.Extensions.Logging;

namespace Common.Challenges;

public class CookieGrabberSolver : IChallengeSolver
{
    private readonly HttpClient client;
    private readonly ILogger<CookieGrabberSolver> logger;

    public CookieGrabberSolver(HttpClient client, ILogger<CookieGrabberSolver> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<(ChallengeSolution solution, CloudflareClearanceResult result)> Solve(Uri uri)
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
                    .Select(CookieUtils.CookieDtoToSystemNetCookie)
                    .ToList()),
            ExpiryTime: expiryTime,
            Origin: new Uri(uri.GetOrigin()));
        
        logger.LogInformation("Found solution {0}", solution);

        return (solution, result);
    }

    async Task<ChallengeSolution> IChallengeSolver.Solve(Uri uri)
    {
        return (await Solve(uri)).solution;
    }

    public void Invalidate(ChallengeSolution solved)
    {
        // do nothing
    }
}