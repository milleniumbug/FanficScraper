using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class AddModel : PageModel
{
    private readonly FanFicUpdater fanFicUpdater;

    public AddModel(
        FanFicUpdater fanFicUpdater)
    {
        this.fanFicUpdater = fanFicUpdater;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string url)
    {
        var id = await fanFicUpdater.UpdateStory(url);
        return RedirectToPage("/StoryDetails", new
        {
            id = id
        });
    }
}