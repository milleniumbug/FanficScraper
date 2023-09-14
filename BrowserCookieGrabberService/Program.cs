using System.Diagnostics;
using BrowserCookieGrabberService;
using BrowserCookieGrabberService.Database.FirefoxCookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var firefoxCookiesConfigurationSection = builder.Configuration.GetSection("FirefoxCookies");
builder.Services.Configure<FirefoxCookiesGrabberConfiguration>(firefoxCookiesConfigurationSection);
var firefoxCookies = firefoxCookiesConfigurationSection.Get<FirefoxCookiesGrabberConfiguration>();

builder.Services.AddDbContextFactory<FirefoxCookiesContext, FirefoxCookiesContextFactory>();
builder.Services.AddSingleton<UserAgentGrabber>();
builder.Services.AddScoped<FirefoxCookieGrabber>(provider =>
{
    return new FirefoxCookieGrabber(
        provider.GetRequiredService<ILogger<FirefoxCookieGrabber>>(),
        provider.GetRequiredService<IDbContextFactory<FirefoxCookiesContext>>(),
        firefoxCookies.SitesAllowed.Split(),
        new string[] { firefoxCookies.CookieNamePattern });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.StartAsync();

var url = app.Urls.First();
var urlBuilder = new UriBuilder(new Uri(url))
{
    Host = "localhost",
    Path = "SetUserAgent"
};
using var firefoxProcess = new Process()
{
    StartInfo = new ProcessStartInfo()
    {
        UseShellExecute = false,
        FileName = "firefox",
        ArgumentList = { urlBuilder.ToString() }
    }
};
firefoxProcess.Start();

await app.WaitForShutdownAsync();