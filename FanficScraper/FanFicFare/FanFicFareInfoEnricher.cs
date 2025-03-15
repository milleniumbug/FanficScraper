using System.Net;
using Common.Scraping;
using ScrapySharp.Extensions;

namespace FanficScraper.FanFicFare;

// decorator over IFanFicFare that adds extra info
// that was not added by the underlying IFanFicFare interface
public class FanFicFareInfoEnricher : IFanFicFare
{
    private IFanFicFare fanFicFare;
    private readonly CachingScraper cachingScraper;

    public FanFicFareInfoEnricher(
        IFanFicFare fanFicFare,
        CachingScraper cachingScraper)
    {
        this.fanFicFare = fanFicFare;
        this.cachingScraper = cachingScraper;
    }

    public async Task<FanFicStoryDetails> Run(Uri storyUrl, bool metadataOnly = false, bool force = false)
    {
        var result = await fanFicFare.Run(storyUrl, metadataOnly, force);
        var extraInfo  = new ExtraInfo(storyUrl, cachingScraper);

        return new FanFicStoryDetails(
            author: result.Author,
            title: result.Title,
            authorId: result.AuthorId,
            license: result.License ?? await extraInfo.GetLicense(),
            storyUrl: result.StoryUrl,
            siteUrl: result.SiteUrl,
            siteAbbreviation: result.SiteAbbreviation,
            isCompleted: result.IsCompleted,
            websiteUpdateDate: result.WebsiteUpdateDate,
            outputFilename: result.OutputFilename,
            numWords: result.NumWords,
            numChapters: result.NumChapters,
            characters: result.Characters,
            category: result.Category,
            genre: result.Genre,
            relationships: result.Relationships,
            warnings: result.Warnings,
            rating: result.Rating,
            descriptionParagraphs: result.DescriptionParagraphs,
            publicationDate: result.PublicationDate,
            isRemoved: result.IsRemoved ?? await extraInfo.GetIsRemoved(),
            isArchived: result.IsArchived);
    }
}

public class ExtraInfo(Uri storyUrl, CachingScraper cachingScraper)
{
    public class MainSiteInfo
    {
        public string? License { get; set; }
        
        public bool? IsRemoved { get; set; }
    }
    
    private MainSiteInfo? mainSiteInfo;

    private async Task<MainSiteInfo> GetMainSiteInfo()
    {
        var (doc, code) = await cachingScraper.GetAsync(storyUrl.ToString());

        string? license = null; 
        switch (StoryUrlNormalizer.GetAbbreviation(storyUrl))
        {
            case "scrhub":
                license = doc.Document.DocumentNode
                    .CssSelect(".copyright .copy_allrights")
                    ?.FirstOrDefault()
                    ?.ParentNode
                    ?.Element("span")
                    ?.GetInnerTextForReal();
                break;
        }
        return new MainSiteInfo
        {
            License = license,
            IsRemoved = code switch
            {
                HttpStatusCode.OK => false,
                HttpStatusCode.NotFound => true,
                _ => null,
            }
        };
    }
        
    public async Task<string?> GetLicense()
    {
        mainSiteInfo ??= await GetMainSiteInfo();
        return mainSiteInfo.License;
    }
    
    public async Task<bool?> GetIsRemoved()
    {
        mainSiteInfo ??= await GetMainSiteInfo();
        return mainSiteInfo.IsRemoved;
    }
}