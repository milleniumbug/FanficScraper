using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class IndexModel : PageModel
{
    private readonly FanFicUpdater fanFicUpdater;
    private readonly StoryBrowser storyBrowser;
    public IEnumerable<Data.Story> UpdatedStories { get; private set; }
        = Enumerable.Empty<Data.Story>();
    
    public IEnumerable<Data.Story>? RecentlyAdded { get; private set; }

    public string Name { get; private set; } = "";

    public IndexModel(
        FanFicUpdater fanFicUpdater,
        StoryBrowser storyBrowser)
    {
        this.fanFicUpdater = fanFicUpdater;
        this.storyBrowser = storyBrowser;
    }

    public async Task OnGetAsync(string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var stories = await this.storyBrowser.FindByName(name);
            this.UpdatedStories = stories;
            this.Name = name;
        }
        else
        {
            var lastUpdated = await this.storyBrowser.FindLastUpdated(15);
            this.UpdatedStories = lastUpdated;
            this.RecentlyAdded = await this.storyBrowser.FindRecentlyAdded(15); 
        }
    }
}