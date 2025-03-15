using System.Net;
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

var backupConfigurationSection = builder.Configuration.GetSection("BackupConfiguration");
builder.Services.Configure<BackupConfiguration>(backupConfigurationSection);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<StoryBrowser>();
builder.Services.AddScoped<FanFicUpdater>();
builder.Services.AddScoped<UserManager>();
builder.Services.AddScoped<BackupService>(provider =>
{
    var dataConfiguration = provider.GetRequiredService<IOptions<DataConfiguration>>();
    return new BackupService(
        provider.GetRequiredService<StoryUpdateLock>(),
        provider.GetRequiredService<IOptions<BackupConfiguration>>(),
        dataConfiguration,
        provider.GetRequiredService<TimeProvider>(),
        provider.GetRequiredService<StoryContext>(),
        dataConfiguration.Value.HasSecondaryInstance
            ? provider.GetRequiredService<FanFicScraperClient>()
            : null);
});
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<PhraseGenerator>();
builder.Services.AddSingleton<StoryUpdateLock>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
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

builder.Services.AddScoped<CachingScraper>(provider =>
{
    HttpClientHandler? httpHandler = null;
    if (dataConfiguration.ProxyUrl != null)
    {
        httpHandler = new HttpClientHandler()
        {
            Proxy = new WebProxy(dataConfiguration.ProxyUrl),
            UseProxy = true,
        };
    }

    var handler = new ChallengeSolverHandler(provider.GetRequiredService<IChallengeSolver>(), httpHandler);
    var client = new HttpClient(handler)
    {
        BaseAddress = new Uri(dataConfiguration.ScribbleHub.ScribbleHubAddress),
        Timeout = Timeout.InfiniteTimeSpan,
    };
        
    return new CachingScraper(
        client,
        new NullCache<string, string>(),
        provider.GetRequiredService<ILogger<CachingScraper>>());
});
builder.Services.AddScoped<ScribbleHubFeed.ScribbleHubFeed>();

if (dataConfiguration.ScribbleHub.EnableScribbleHubFeed)
{
    builder.Services.AddHostedService<ScribbleHubFeedUpdaterService>();
}

if (dataConfiguration.HasSecondaryInstance)
{
    builder.Services.AddScoped<FanFicScraperClient>(provider =>
        new FanFicScraperClient(
            new HttpClient()
            {
                BaseAddress = new Uri(dataConfiguration.SecondaryFanFicScraperUrl),
                Timeout = TimeSpan.FromMinutes(30)
            }));
}

builder.Services.AddScoped<IFanFicFare>(provider =>
{
    var clients = new List<IFanFicFare>();
    if (dataConfiguration.HasSecondaryInstance)
    {
        clients.Add(new FanFicScraper(
            provider.GetRequiredService<FanFicScraperClient>(),
            Options.Create(dataConfiguration),
            provider.GetRequiredService<ILogger<FanFicScraper>>()));
    }
    else
    {
        clients.Add(
            new FanFicFareInfoEnricher(
                new FanFicFare(
                    new FanFicFareSettings()
                    {
                        IsAdult = true,
                        TargetDirectory = dataConfiguration.StoriesDirectory,
                        IncludeImages = true,
                        FanFicFareExecutablePath = dataConfiguration.FanFicFareExecutablePath,
                        ChallengeSolver = provider.GetService<IChallengeSolver>(),
                        ProxyUrl = dataConfiguration.ProxyUrl,
                    },
                    provider.GetRequiredService<ILogger<FanFicFare>>()),
                provider.GetRequiredService<CachingScraper>()));

    }

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