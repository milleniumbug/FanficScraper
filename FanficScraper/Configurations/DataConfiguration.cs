namespace FanficScraper.Configurations
{
    public class DataConfiguration
    {
        public string ConnectionString { get; set; }
        
        public string StoriesDirectory { get; set; }
        
        public string SecondaryFanFicScraperUrl { get; set; }
        
        public string SecondaryFanFicScraperPassphrase { get; set; }
        
        public bool DisableAutoUpdate { get; set; }
    }
}