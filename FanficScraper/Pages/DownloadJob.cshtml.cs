using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class DownloadJobModel : PageModel
{
    private readonly FanFicUpdater updater;

    public DownloadJobModel(
        FanFicUpdater updater)
    {
        this.updater = updater;
    }

    public async Task OnGetAsync(
        [FromRoute] string id)
    {
        var ids = id.Split(",")
            .Select(i => Guid.TryParse(i, out var guid) ? guid : (Guid?)null)
            .Where(guid => guid.HasValue)
            .Select(guid => guid!.Value)
            .ToList();
        var (jobs, status) = await this.updater.GetScheduledJobsDetails(ids);
        this.Jobs = jobs;
        this.AggregateStatus = status;
    }

    public IReadOnlyCollection<JobDetails> Jobs { get; set; } = Array.Empty<JobDetails>();
    
    public AggregateDownloadJobStatus AggregateStatus { get; set; }
}