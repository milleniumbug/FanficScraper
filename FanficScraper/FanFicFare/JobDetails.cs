using FanficScraper.Data;

namespace FanficScraper.FanFicFare;

public class JobDetails
{
    public DownloadJobStatus Status { get; }
    public string Url { get; }
    public string? StoryId { get; }

    public JobDetails(DownloadJobStatus status, string url, string? storyId)
    {
        Status = status;
        Url = url;
        StoryId = storyId;
    }
}