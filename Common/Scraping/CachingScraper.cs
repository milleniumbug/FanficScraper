#nullable enable
using System.Net;
using System.Web;
using Common.Caching;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ScrapySharp.Network;

namespace Common.Scraping;

public class CachingScraper
{
    private readonly ICache<string, string> cache;
    private readonly ILogger<CachingScraper> logger;
    private readonly HttpClient client;

    public CachingScraper(
        HttpClient client,
        ICache<string, string> cache,
        ILogger<CachingScraper> logger)
    {
        this.client = client;
        this.cache = cache;
        this.logger = logger;
    }

    public async Task<WebDocument> DownloadAsync(string url)
    {
        var html = new HtmlDocument();

        this.logger.LogInformation("Attempting to scrape {Url}", url);
        var rawHtml = await cache.Get(url);
        
        if (rawHtml != null)
        {
            this.logger.LogInformation("Url was found in cache {Url}", url);
        }
        else
        {
            this.logger.LogInformation("Url was NOT found in cache, issuing manual download {Url}", url);
            var response = await client.GetAsync(url);
            this.logger.LogInformation("Manual download issued for URL {Url} (Status Code = {StatusCode}", url, response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                rawHtml = await response.Content.ReadAsStringAsync();
                await cache.Set(url, rawHtml);
            }
            else
            {
                rawHtml =
                    $"<html>{HttpUtility.HtmlEncode(response.StatusCode.ToString())}</html>";
                await cache.Set(url, rawHtml);
            }
        }
        
        this.logger.LogInformation("Attempting to parse HTML");
        html.LoadHtml(rawHtml);

        return new WebDocument(html, url);
    }
}