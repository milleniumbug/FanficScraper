namespace FanficScraper.FanFicFare;

public class FanFicStoryDetails
{
    public string Author { get; }
    public string Title { get; }
    public DateTime PublicationDate { get; }
    public DateTime WebsiteUpdateDate { get; }
    public string OutputFilename { get; }
    public string NumChapters { get; }
    public string NumWords { get; }
    public string SiteUrl { get; }
    public string SiteAbbreviation { get; }
    public string StoryUrl { get; }
    public bool IsCompleted { get; }

    public FanFicStoryDetails(
        string author,
        string title,
        DateTime publicationDate,
        DateTime websiteUpdateDate,
        string outputFilename,
        string numChapters,
        string numWords,
        string siteUrl,
        string siteAbbreviation,
        string storyUrl,
        bool isCompleted)
    {
        Author = author;
        Title = title;
        PublicationDate = publicationDate;
        WebsiteUpdateDate = websiteUpdateDate;
        OutputFilename = outputFilename;
        NumChapters = numChapters;
        NumWords = numWords;
        SiteUrl = siteUrl;
        SiteAbbreviation = siteAbbreviation;
        StoryUrl = storyUrl;
        IsCompleted = isCompleted;
    }
}