namespace FanficScraper.Data;

public class DownloadJob
{
    public Guid Id { get; set; }
    
    public DownloadJobStatus Status { get; set; }

    public bool Force { get; set; }
    
    public string Url { get; set; }
    
    public string? FileName { get; set; }
    
    public DateTime AddedDate { get; set; }
    
    public DateTime? FinishDate { get; set; }
    
    public Guid? RunnerId { get; set; }
}