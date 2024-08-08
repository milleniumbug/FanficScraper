using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Common;
using Common.Api;
using Common.Challenges;
using Common.Crypto;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ScribbleHubFeed;
using Xunit.Abstractions;

namespace TestProject1;

public class Sandbox
{
    private readonly ITestOutputHelper testOutputHelper;

    public Sandbox(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Json()
    {
        var s = new SeriesFinderSettings()
        {
            Status = StoryStatus.All,
            SortDirection = SortOrder.Descending,
            SortBy = SortCriteria.DateAdded,
            TagInclusion = Alternative.And,
            IncludedTags = new []{ Tags.Parse("Transgender") }
        };

        var result = System.Text.Json.JsonSerializer.Serialize(s);
        var r = System.Text.Json.JsonSerializer.Serialize(result);
    }

    [Fact(Skip = "needs to be run explicitly")]
    public async Task Sample()
    {
        var cacheSolver = new FilteringChallengeSolver(
            FilteringChallengeSolver.InclusionType.SolveAllChallengesExceptOnTheList,
            new []{ "https://www.tgstorytime.com" },
            new CachingChallengeSolver(
                new FlareSolverr(new HttpClient()
                    {
                        BaseAddress = new Uri("http://localhost:8191"),
                        Timeout = TimeSpan.FromMinutes(2)
                    }),
                    TimeSpan.FromMinutes(5),
                NullLogger<CachingChallengeSolver>.Instance));

        var v1 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com"));
        ;
        var v2 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/443703/the-harem-protagonist-was-turned-into-a-girl-and-doesnt-want-to-change-back/"));
        ;
        var v3 = await cacheSolver.Solve(new Uri("https://www.scribblehub.com/series/194683/acceptance-of-the-self/"));
        ;
        var v4 = await cacheSolver.Solve(new Uri("https://www.tgstorytime.com/viewstory.php?sid=4160"));
        ;
        PrintOutCookies(v2);
        PrintOutCookies(v4);
    }
    
    [Fact(Skip = "skip")]
    public async Task IssueBackup()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = 
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7091"),
        };
        var response = await httpClient.GetAsync("Api/Backup?key=age1nxanrjpp60c85z6kqykqnxzxnykqy932dkme9waqqwx06zry6q7qvkj9ug");
        var content = await response.Content.ReadAsStreamAsync();
        await using var outputFile =
            File.OpenWrite("encryted.tar.gz.enc");
        await content.CopyToAsync(outputFile);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private void PrintOutCookies(ChallengeSolution solution)
    {
        using var memory = new MemoryStream();
        CookieUtils.WriteCookiesInMozillaFormat(memory, solution.Cookies);

        var s = Encoding.UTF8.GetString(memory.ToArray());
        testOutputHelper.WriteLine(s);
    }
}