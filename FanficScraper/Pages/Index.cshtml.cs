using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class IndexModel : PageModel
{
    private readonly FanFicUpdater fanFicUpdater;
    private readonly StoryBrowser storyBrowser;
    public IEnumerable<Story> UpdatedStories { get; private set; }
        = Enumerable.Empty<Story>();

    public IndexModel(
        FanFicUpdater fanFicUpdater,
        StoryBrowser storyBrowser)
    {
        this.fanFicUpdater = fanFicUpdater;
        this.storyBrowser = storyBrowser;
    }

    public async Task OnGetAsync()
    {
        var lastUpdated = await this.storyBrowser.FindLastUpdated(15);
        this.UpdatedStories = lastUpdated;
    }

    public async Task<IActionResult> OnPostAsync(string url)
    {
        await fanFicUpdater.UpdateStory(url);
        return RedirectToPage();
    }
}