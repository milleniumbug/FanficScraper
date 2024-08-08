using System.Diagnostics.CodeAnalysis;

namespace FanficScraper.Configurations
{
    public class DataConfiguration
    {
        public string ConnectionString { get; set; }
        
        public string StoriesDirectory { get; set; }

        [MemberNotNullWhen(true, nameof(SecondaryFanFicScraperUrl))]
        public bool HasSecondaryInstance => !string.IsNullOrWhiteSpace(SecondaryFanFicScraperUrl);
        
        public string? SecondaryFanFicScraperUrl { get; set; }
        
        public string? SecondaryFanFicScraperPassphrase { get; set; }

        public int MinimumUpdateDistanceInSecondsLowerBound { get; set; } = 30;
        
        public int MinimumUpdateDistanceInSecondsUpperBound { get; set; } = 60;
        
        public bool DisableAutoUpdate { get; set; }

        public FlareSolverrConfiguration FlareSolverr { get; set; } = new();
        
        public CookieGrabberConfiguration CookieGrabber { get; set; } = new();
        
        public ScribbleHubFeedConfiguration ScribbleHub { get; set; } = new();
        
        public string? FanFicFareExecutablePath { get; set; }
    }
}