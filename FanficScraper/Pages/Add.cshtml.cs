using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class AddModel : PageModel
{
    private readonly FanFicUpdater fanFicUpdater;
    private readonly UserManager userManager;

    public AddModel(
        FanFicUpdater fanFicUpdater,
        UserManager userManager)
    {
        this.fanFicUpdater = fanFicUpdater;
        this.userManager = userManager;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string url, string passphrase)
    {
        var result = await userManager.IsAuthorized(passphrase);

        switch (result)
        {
            case AuthorizationResult.Failure:
                return RedirectToPage("/Message", new
                {
                    id = "NotAValidPhrase"
                });
            case AuthorizationResult.NeedsActivation:
                return RedirectToPage("/Message", new
                {
                    id = "UserNotActivated"
                });
            case AuthorizationResult.Success:
            {
                var id = await fanFicUpdater.ScheduleUpdateStory(url, force: true);
                return RedirectToPage("/DownloadJob", new
                {
                    id = id
                });
            }
        }

        throw new InvalidOperationException();
    }
}