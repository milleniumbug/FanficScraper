namespace Common.Api;

public class GetStoryAsyncQueryResponse
{
    public Guid JobId { get; }
    
    public string? StoryId { get; }
    
    public DownloadJobStatus Status { get; }
    
    public string Url { get; }

    public GetStoryAsyncQueryResponse(Guid jobId,
        string? storyId,
        DownloadJobStatus status,
        string url)
    {
        JobId = jobId;
        StoryId = storyId;
        Status = status;
        Url = url;
    }
}