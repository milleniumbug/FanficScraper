using System.Security.Claims;
using System.Security.Cryptography;
using Common;
using Common.Caching;
using Common.Challenges;
using Common.Scraping;
using FanficScraper;
using FanficScraper.Configurations;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var dataConfigurationSection = builder.Configuration.GetSection("DataConfiguration");
builder.Services.Configure<DataConfiguration>(dataConfigurationSection);
var dataConfiguration = dataConfigurationSection.Get<DataConfiguration>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<StoryBrowser>();
builder.Services.AddScoped<FanFicUpdater>();
builder.Services.AddScoped<UserManager>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<PhraseGenerator>();
builder.Services.TryAddSingleton(_ =>
    RandomNumberGenerator.Create());

if (dataConfiguration.FlareSolverr.EnableFlareSolverr && dataConfiguration.CookieGrabber.EnableCookieGrabber)
{
    throw new InvalidDataException("both can't be used at the same time");
}

if (dataConfiguration.FlareSolverr.EnableFlareSolverr)
{
    var c = dataConfiguration.FlareSolverr;
    var policy = c.UrlsToSolve == null
        ? FilteringChallengeSolver.InclusionType.SolveAllChallengesExceptOnTheList
        : FilteringChallengeSolver.InclusionType.SolveNoChallengeExceptOnTheList;

    var urlsToSolve = c.UrlsToSolve == null
        ? Array.Empty<string>()
        : c.UrlsToSolve.Split(";",
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    
    builder.Services.AddSingleton<IChallengeSolver>(provider =>
        new FilteringChallengeSolver(
            policy,
            urlsToSolve,
            new CachingChallengeSolver(
                new FlareSolverr(
                    new HttpClient()
                    {
                        BaseAddress = new Uri(c.Address),
                        Timeout = TimeSpan.FromMilliseconds(c.TimeoutInMilliseconds)
                    }),
                TimeSpan.FromMinutes(5),
                provider.GetRequiredService<ILogger<CachingChallengeSolver>>())));
}

if (dataConfiguration.CookieGrabber.EnableCookieGrabber)
{
    var c = dataConfiguration.CookieGrabber;
    var policy = c.UrlsToSolve == null
        ? FilteringChallengeSolver.InclusionType.SolveAllChallengesExceptOnTheList
        : FilteringChallengeSolver.InclusionType.SolveNoChallengeExceptOnTheList;

    var urlsToSolve = c.UrlsToSolve == null
        ? Array.Empty<string>()
        : c.UrlsToSolve.Split(";",
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    FilteringChallengeSolver MakeSolver(IServiceProvider provider, string url)
    {
        var logger = provider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Creating a solver for {Url}", url);
        return new FilteringChallengeSolver(
            policy,
            urlsToSolve,
            new CookieGrabberSolver(
                new HttpClient()
                {
                    BaseAddress = new Uri(url),
                    Timeout = TimeSpan.FromMilliseconds(c.TimeoutInMilliseconds)
                },
                provider.GetRequiredService<ILogger<CookieGrabberSolver>>()));
    }

    builder.Services.AddSingleton<IChallengeSolver>(provider => 
        new CompositeChallengeSolver(
            c.Address.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(url => MakeSolver(provider, url)),
            provider.GetRequiredService<ILogger<CompositeChallengeSolver>>()));
}

if (dataConfiguration.ScribbleHub.EnableScribbleHubFeed)
{
    var client = new HttpClient()
    {
        Timeout = TimeSpan.FromMilliseconds(dataConfiguration.ScribbleHub.TimeoutInMilliseconds)
    };
    builder.Services.AddScoped<CachingScraper>(provider =>
    {
        return new CachingScraper(
            client,
            new NullCache<string, string>());
    });
    builder.Services.AddScoped<ScribbleHubFeed.ScribbleHubFeed>();
}

builder.Services.AddScoped<IFanFicFare>(provider =>
{
    var clients = new List<IFanFicFare>();
    if (!string.IsNullOrWhiteSpace(dataConfiguration.SecondaryFanFicScraperUrl))
    {
        clients.Add(new FanFicScraper(
            new FanFicScraperClient(
                new HttpClient()
                {
                    BaseAddress = new Uri(dataConfiguration.SecondaryFanFicScraperUrl),
                    Timeout = TimeSpan.FromMinutes(30)
                }),
            Options.Create(dataConfiguration),
            provider.GetRequiredService<ILogger<FanFicScraper>>()));
    }

    clients.Add(new FanFicFare(new FanFicFareSettings()
    {
        IsAdult = true,
        TargetDirectory = dataConfiguration.StoriesDirectory,
        IncludeImages = true,
        FanFicFareExecutablePath = dataConfiguration.FanFicFareExecutablePath,
        ChallengeSolver = provider.GetService<IChallengeSolver>()
    }, provider.GetRequiredService<ILogger<FanFicFare>>()));

    return new CompositeFanFicFare(clients, provider.GetRequiredService<ILogger<CompositeFanFicFare>>());
});

builder.Services.AddSqlite<StoryContext>(dataConfiguration.ConnectionString);

if (!dataConfiguration.DisableAutoUpdate)
{
    builder.Services.AddHostedService<FanFicAutoUpdaterService>();
}

builder.Services.AddHostedService<FanFicManualUpdaterService>();


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();