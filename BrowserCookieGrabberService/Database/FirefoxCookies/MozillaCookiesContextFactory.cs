using Microsoft.EntityFrameworkCore;

namespace BrowserCookieGrabberService.Database.FirefoxCookies;

public class FirefoxCookiesContextFactory : IDbContextFactory<FirefoxCookiesContext>
{
    public FirefoxCookiesContextFactory(DbContextOptions options)
    {
    }
    
    public FirefoxCookiesContext CreateDbContext()
    {
        var cookieSqlitePath = "/home/milleniumbug/.mozilla/firefox/dcsaxgd2.default/cookies.sqlite";
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
        File.Copy(cookieSqlitePath, path);
        var connectionString = $"Data Source={path};Mode=ReadOnly";
        return new FirefoxCookiesContext(
            new DbContextOptionsBuilder<FirefoxCookiesContext>()
                .UseSqlite(connectionString).Options,
            path);
    }
}