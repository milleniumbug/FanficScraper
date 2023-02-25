namespace FanficScraper.Configurations;

public class FlareSolverrConfiguration
{
    public bool EnableFlareSolverr { get; set; } = false;

    public string Address { get; set; } = "http://localhost:8191";

    public int TimeoutInMilliseconds { get; set; } = 60000;
}