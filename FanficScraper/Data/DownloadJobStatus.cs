namespace FanficScraper.Data;

public enum DownloadJobStatus
{
    NotYetStarted,
    Failed,
    Succeeded,
    Started
}

public enum AggregateDownloadJobStatus
{
    NotYetStarted,
    InProgress,
    InProgressWithErrors,
    FinishedSuccessfully,
    FinishedWithErrors,
    Failed
}