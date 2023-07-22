namespace Common.Models;

public class CloudflareClearanceResult
{
    public string? UserAgent { get; set; }
    
    public string? CookiesMozillaFormat { get; set; }
    
    public IEnumerable<CookieDto> Cookies { get; set; }
}