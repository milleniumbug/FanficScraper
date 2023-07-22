using BrowserCookieGrabberService;
using BrowserCookieGrabberService.Database.FirefoxCookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<FirefoxCookiesContext, FirefoxCookiesContextFactory>();
builder.Services.AddSingleton<UserAgentGrabber>();
builder.Services.AddScoped<FirefoxCookieGrabber>(provider =>
{
    return new FirefoxCookieGrabber(
        provider.GetRequiredService<ILogger<FirefoxCookieGrabber>>(),
        provider.GetRequiredService<IDbContextFactory<FirefoxCookiesContext>>(),
        new string[] { "https://scribblehub.com", "https://www.scribblehub.com" },
        new string[] { "*cf*" });
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

app.Run();