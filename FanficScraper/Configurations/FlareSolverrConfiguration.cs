namespace FanficScraper.Configurations;

public class FlareSolverrConfiguration
{
    public bool EnableFlareSolverr { get; set; } = false;

    public string Address { get; set; } = "localhost";

    public int Port { get; set; } = 8191;

    public string Protocol { get; set; } = "http";

    public int TimeoutInMilliseconds { get; set; } = 60000;
}