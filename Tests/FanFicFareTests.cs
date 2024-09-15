using Common.Challenges;
using FanficScraper.FanFicFare;
using Meziantou.Extensions.Logging.Xunit;
using Xunit.Abstractions;

namespace TestProject1;

public class FanFicFareTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public FanFicFareTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task FanFicFare()
    {
        var fanficfare = new FanFicFare(
            new FanFicFareSettings()
            {
                FanFicFareExecutablePath = "/home/milleniumbug/dokumenty/PROJEKTY/NotMine/fanficfare/fanficfare",
            },
            XUnitLogger.CreateLogger<FanFicFare>(this.testOutputHelper));

        var matcher = await fanficfare.GetSupportedSiteMatcher();
        matcher(new Uri("https://scribblehub.com"));
    }
}