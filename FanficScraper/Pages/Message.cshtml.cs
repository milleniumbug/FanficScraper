using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FanficScraper.Pages;

public class MessageModel : PageModel
{
    private static readonly Dictionary<string, string> errors = new()
    {
        {"UserNotActivated", "The user using this passphrase is not activated. Send the activation phrase to the server owner."},
        {"NotAValidPhrase", "The supplied phrase is not valid"}
    };
    
    public string ErrorMessage { get; private set; }

    public MessageModel()
    {
        
    }

    public async Task OnGetAsync(
        [FromRoute] string id)
    {
        this.ErrorMessage = errors[id];
    }
}