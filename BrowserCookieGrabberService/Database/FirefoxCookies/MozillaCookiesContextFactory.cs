using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BrowserCookieGrabberService.Database.FirefoxCookies;

public class FirefoxCookiesContextFactory : IDbContextFactory<FirefoxCookiesContext>
{
    private readonly FirefoxCookiesGrabberConfiguration config;

    public FirefoxCookiesContextFactory(
        DbContextOptions options,
        IOptions<FirefoxCookiesGrabberConfiguration> config)
    {
        this.config = config.Value;
    }
    
    public FirefoxCookiesContext CreateDbContext()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
        File.Copy(config.PathToProfile, path);
        var connectionString = $"Data Source={path};Mode=ReadOnly";
        return new FirefoxCookiesContext(
            new DbContextOptionsBuilder<FirefoxCookiesContext>()
                .UseSqlite(connectionString).Options,
            path);
    }
}