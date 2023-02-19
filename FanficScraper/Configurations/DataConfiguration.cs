namespace FanficScraper.Configurations
{
    public class DataConfiguration
    {
        public string ConnectionString { get; set; }
        
        public string StoriesDirectory { get; set; }
        
        public string? SecondaryFanFicScraperUrl { get; set; }
        
        public string? SecondaryFanFicScraperPassphrase { get; set; }

        public int MinimumUpdateDistanceInSecondsLowerBound { get; set; } = 30;
        
        public int MinimumUpdateDistanceInSecondsUpperBound { get; set; } = 60;
        
        public bool DisableAutoUpdate { get; set; }
    }
}