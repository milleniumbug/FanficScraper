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
        [FromRoute] Guid id)
    {
        this.JobId = id;
        this.Job = await this.updater.GetScheduledJobDetails(id);
    }

    public JobDetails? Job { get; set; }
    
    public Guid JobId { get; set; }
}