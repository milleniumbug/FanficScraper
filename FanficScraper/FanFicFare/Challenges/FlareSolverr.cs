using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FanficScraper.Configurations;

namespace FanficScraper.FanFicFare.Challenges;

public class FlareSolverr : IChallengeSolver
{
    private readonly HttpClient client;
    private readonly FlareSolverrConfiguration configuration;

    public FlareSolverr(HttpClient client, FlareSolverrConfiguration configuration)
    {
        this.client = client;
        this.configuration = configuration;
    }
    
    public async Task<ChallengeResult> Solve(Uri uri)
    {
        var response = await this.client.PostAsJsonAsync("/v1", new FlareSolverrGetRequest()
        {
            Url = uri.ToString(),
            ReturnOnlyCookies = true,
            MaxTimeout = this.configuration.TimeoutInMilliseconds
        });

        var flareSolverrGetResponse = await JsonSerializer.DeserializeAsync<FlareSolverrGetResponse>(
            await response.Content.ReadAsStreamAsync());

        if (flareSolverrGetResponse?.Solution == null || flareSolverrGetResponse.Status != "ok")
        {
            throw new FlareSolverrException(flareSolverrGetResponse?.Message);
        }

        var expiryTimeInSeconds = flareSolverrGetResponse.Solution.Cookies
            .FirstOrDefault(cookie => cookie.Name == "cf_clearance")?.Expires;
        var expiryTime = expiryTimeInSeconds == null
            ? DateTime.UtcNow.AddDays(14)
            : DateTimeOffset.FromUnixTimeSeconds((long)expiryTimeInSeconds).UtcDateTime;

        return new ChallengeResult(
            UserAgent: flareSolverrGetResponse.Solution.UserAgent,
            Cookies: MapCookies(flareSolverrGetResponse.Solution.Cookies),
            Origin: new Uri(uri.GetLeftPart(UriPartial.Authority)),
            ExpiryTime: expiryTime);
    }

    private IReadOnlyList<Cookie> MapCookies(IReadOnlyList<FlareSolverrCookie> solutionCookies)
    {
        return solutionCookies
            .Select(cookie => new Cookie()
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Expires = DateTimeOffset.FromUnixTimeSeconds((long)cookie.Expires).UtcDateTime,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Domain = cookie.Domain,
                Path = cookie.Path
            })
            .ToList();
    }
}

public class FlareSolverrException : Exception
{
    public FlareSolverrException(string? message) : base(message)
    {
        
    }
}

public class FlareSolverrGetRequest
{
    [JsonPropertyName("cmd")]
    public string Cmd { get; } = "request.get";
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("session")]
    public string? Session { get; set; }
    
    [JsonPropertyName("maxTimeout")]
    public int? MaxTimeout { get; set; }

    [JsonPropertyName("returnOnlyCookies")]
    public bool? ReturnOnlyCookies { get; set; }
}

public class FlareSolverrGetResponse
{
    [JsonPropertyName("solution")]
    public FlareSolverrSolution? Solution { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
}

public class FlareSolverrSolution
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("cookies")]
    public IReadOnlyList<FlareSolverrCookie> Cookies { get; set; }
    
    [JsonPropertyName("userAgent")]
    public string UserAgent { get; set; }
}

public class FlareSolverrCookie
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("value")]
    public string Value { get; set; }
    
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    
    [JsonPropertyName("path")]
    public string Path { get; set; }
    
    [JsonPropertyName("expiry")]
    public double Expires { get; set; }
    
    [JsonPropertyName("size")]
    public int Size { get; set; }
    
    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; set; }
    
    [JsonPropertyName("secure")]
    public bool Secure { get; set; }
    
    [JsonPropertyName("session")]
    public bool Session { get; set; }
    
    [JsonPropertyName("sameSite")]
    public string SameSite { get; set; }
}

