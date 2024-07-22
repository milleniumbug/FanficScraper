using FanficScraper.Data;

namespace FanficScraper.FanFicFare;

public class JobDetails
{
    public DownloadJobStatus Status { get; }
    public string Url { get; }
    public string? StoryId { get; }
    public Guid JobId { get; }

    public JobDetails(Guid jobId, DownloadJobStatus status, string url, string? storyId)
    {
        JobId = jobId;
        Status = status;
        Url = url;
        StoryId = storyId;
    }
}