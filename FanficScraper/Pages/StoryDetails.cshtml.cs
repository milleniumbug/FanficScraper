using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class StoryDetailsModel : PageModel
{
    private readonly StoryBrowser storyBrowser;

    public StoryDetailsModel(
        StoryBrowser storyBrowser)
    {
        this.storyBrowser = storyBrowser;
    }

    public async Task OnGetAsync(
        [FromRoute] string id)
    {
        this.Story = await this.storyBrowser.FindById(id);
    }

    public Data.Story? Story { get; private set; }
}