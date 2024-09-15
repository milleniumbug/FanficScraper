using Common.Challenges;
using FanficScraper.Configurations;

namespace FanficScraper.FanFicFare;

public class FanFicFareSettings
{
    public bool IsAdult { get; set; } = false;
    
    public bool IncludeImages { get; set; } = false;

    public string? TargetDirectory { get; set; }

    public string? FanFicFareExecutablePath { get; set; }

    public IChallengeSolver? ChallengeSolver { get; set; }
    
    public string? ProxyUrl { get; set; }
}