using FanficScraper.Configurations;
using FanficScraper.FanFicFare.Challenges;

namespace FanficScraper.FanFicFare;

public class FanFicFareSettings
{
    public bool IsAdult { get; set; } = false;
    
    public bool IncludeImages { get; set; } = false;

    public string? TargetDirectory { get; set; }

    public IChallengeSolver? ChallengeSolver { get; set; }
}