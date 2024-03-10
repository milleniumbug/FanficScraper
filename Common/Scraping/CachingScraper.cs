#nullable enable
using System.Net;
using System.Web;
using Common.Caching;
using HtmlAgilityPack;
using ScrapySharp.Network;

namespace Common.Scraping;

public class CachingScraper
{
    private readonly ICache<string, string> cache;
    private readonly HttpClient client;

    public CachingScraper(
        HttpClient client,
        ICache<string, string> cache)
    {
        this.client = client;
        this.cache = cache;
    }

    public async Task<WebDocument> DownloadAsync(string url)
    {
        var html = new HtmlDocument();

        var rawHtml = await cache.Get(url);
        if(rawHtml != null)
        {
            html.LoadHtml(rawHtml);
        }
        else
        {
            var response = await client.GetAsync(url);
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
            
            html.LoadHtml(rawHtml);
        }

        return new WebDocument(html, url);
    }
}