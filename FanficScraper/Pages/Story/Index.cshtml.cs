using FanficScraper.Configurations;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FanficScraper.Pages.Story;

public class IndexModel : PageModel
{
    private readonly StoryBrowser storyBrowser;
    private readonly DataConfiguration dataConfiguration;

    [BindProperty(SupportsGet =  true)]
    [FromRoute]
    public string File { get; set; }

    public IndexModel(
        StoryBrowser storyBrowser,
        IOptions<DataConfiguration> dataConfiguration)
    {
        this.storyBrowser = storyBrowser;
        this.dataConfiguration = dataConfiguration.Value;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return File(
            System.IO.File.OpenRead(Path.Combine(this.dataConfiguration.StoriesDirectory, File)),
            "application/octet-stream",
            File);
    }
}