using FanficScraper;
using FanficScraper.Configurations;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var dataConfigurationSection = builder.Configuration.GetSection("DataConfiguration");
builder.Services.Configure<DataConfiguration>(dataConfigurationSection);
var dataConfiguration = dataConfigurationSection.Get<DataConfiguration>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<StoryBrowser>();
builder.Services.AddScoped<FanFicUpdater>();
builder.Services.AddScoped<IFanFicFare, FanFicFare>(provider => new FanFicFare(new FanFicFareSettings()
{
    IsAdult = true,
    TargetDirectory = dataConfiguration.StoriesDirectory
}));

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