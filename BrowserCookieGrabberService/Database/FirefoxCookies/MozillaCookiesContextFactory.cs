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
        var connectionString = $"Data Source={cookieSqlitePath};Mode=ReadOnly";
        return new FirefoxCookiesContext(
            new DbContextOptionsBuilder<FirefoxCookiesContext>()
                .UseSqlite(connectionString).Options);
    }
}