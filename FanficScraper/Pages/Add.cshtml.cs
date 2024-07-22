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

    public async Task<IActionResult> OnPostAsync(string? url, string? passphrase)
    {
        if (url == null)
        {
            return RedirectToPage("/Add");
        }

        if (passphrase == null)
        {
            return RedirectToPage("/Message", new
            {
                id = "NotAValidPhrase"
            });
        }
        
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
                var urls = url.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var urlsToIdsMapping = await fanFicUpdater.ScheduleUpdateStories(urls, force: true);
                return RedirectToPage("/DownloadJob", new
                {
                    id = string.Join(",", urlsToIdsMapping.Select(kvp => kvp.Value))
                });
            }
        }

        throw new InvalidOperationException();
    }
}