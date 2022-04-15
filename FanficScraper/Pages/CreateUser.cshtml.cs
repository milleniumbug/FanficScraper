using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class CreateUserModel : PageModel
{
    private readonly UserManager userManager;
    
    public string? ActivationPhrase { get; private set; }
    
    public string? PassPhrase { get; private set; }

    public CreateUserModel(
        UserManager userManager)
    {
        this.userManager = userManager;
    }

    public async Task OnPostAsync()
    {
        (ActivationPhrase, PassPhrase) = await this.userManager.CreateUser();
    }
}