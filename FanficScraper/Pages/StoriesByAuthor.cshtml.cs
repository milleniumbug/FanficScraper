using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class StoriesByAuthorModel : PageModel
{
    private readonly FanFicUpdater fanFicUpdater;
    private readonly StoryBrowser storyBrowser;
    
    public IEnumerable<Data.Story> Stories { get; private set; }
        = Enumerable.Empty<Data.Story>();

    public string Name { get; private set; } = "";

    public StoriesByAuthorModel(
        FanFicUpdater fanFicUpdater,
        StoryBrowser storyBrowser)
    {
        this.fanFicUpdater = fanFicUpdater;
        this.storyBrowser = storyBrowser;
    }

    public async Task OnGetAsync([FromRoute] string? authorName)
    {
        if (!string.IsNullOrWhiteSpace(authorName))
        {
            this.Name = authorName;
            this.Stories = await this.storyBrowser.FindByAuthor(authorName);
        }
    }
}