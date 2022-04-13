using FanficScraper;
using FanficScraper.Configurations;
using FanficScraper.Data;

var builder = WebApplication.CreateBuilder(args);

var databaseConfigurationSection = builder.Configuration.GetSection("DataConfiguration");
builder.Services.Configure<DataConfiguration>(databaseConfigurationSection);
var databaseConfiguration = databaseConfigurationSection.Get<DataConfiguration>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSqlite<StoryContext>(databaseConfiguration.ConnectionString);

var app = builder.Build();

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

app.MapRazorPages();

app.Run();