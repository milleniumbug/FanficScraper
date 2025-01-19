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
    
    public class MainSiteInfo
    {
        public string? License { get; set; }
    }

    private class ExtraInfo(Uri storyUrl, CachingScraper cachingScraper)
    {
        private MainSiteInfo? mainSiteInfo;

        private async Task<MainSiteInfo> GetMainSiteInfo()
        {
            var page = await cachingScraper.DownloadAsync(storyUrl.ToString());

            return new MainSiteInfo
            {
                License = page.Document.DocumentNode
                    .CssSelect(".copyright .copy_allrights")
                    ?.FirstOrDefault()
                    ?.ParentNode
                    ?.Element("span")
                    ?.GetInnerTextForReal()
            };
        }
        
        public async Task<string?> GetLicense()
        {
            mainSiteInfo = mainSiteInfo ?? await GetMainSiteInfo();
            return mainSiteInfo.License;
        }
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
            publicationDate: result.PublicationDate);
    }
}