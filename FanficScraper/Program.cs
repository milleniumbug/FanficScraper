using FanficScraper;
using FanficScraper.Configurations;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var dataConfigurationSection = builder.Configuration.GetSection("DataConfiguration");
builder.Services.Configure<DataConfiguration>(dataConfigurationSection);
var dataConfiguration = dataConfigurationSection.Get<DataConfiguration>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<StoryBrowser>();
builder.Services.AddScoped<FanFicUpdater>();
builder.Services.AddScoped<IFanFicFare>(provider =>
{
    var clients = new List<IFanFicFare>();
    if (!string.IsNullOrWhiteSpace(dataConfiguration.SecondaryFanFicScraperUrl))
    {
        clients.Add(new FanFicScraper(new HttpClient()
        {
            BaseAddress = new Uri(dataConfiguration.SecondaryFanFicScraperUrl)
        }, Options.Create(dataConfiguration)));
    }
    clients.Add(new FanFicFare(new FanFicFareSettings()
    {
        IsAdult = true,
        TargetDirectory = dataConfiguration.StoriesDirectory
    }));

    return new CompositeFanFicFare(clients);
});

builder.Services.AddSqlite<StoryContext>(dataConfiguration.ConnectionString);

builder.Services.AddHostedService<FanFicUpdaterService>();


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