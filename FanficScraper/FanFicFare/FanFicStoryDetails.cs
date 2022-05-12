namespace FanficScraper.FanFicFare;

public class FanFicStoryDetails
{
    public string Author { get; }
    public string Title { get; }
    public DateTime PublicationDate { get; }
    public DateTime WebsiteUpdateDate { get; }
    public string OutputFilename { get; }
    public int? NumChapters { get; }
    public int? NumWords { get; }
    public string SiteUrl { get; }
    public string SiteAbbreviation { get; }
    public string StoryUrl { get; }
    public bool IsCompleted { get; }
    public IReadOnlyList<string>? Category { get; }
    public IReadOnlyList<string>? Characters { get; }
    public IReadOnlyList<string>? Genre { get; }
    public IReadOnlyList<string>? Relationships { get; }
    public string? Rating { get; }
    public IReadOnlyList<string>? Warnings { get; }
    public IReadOnlyList<string>? DescriptionParagraphs { get; }

    public FanFicStoryDetails(
        string author,
        string title,
        DateTime publicationDate,
        DateTime websiteUpdateDate,
        string outputFilename,
        int? numChapters,
        int? numWords,
        string siteUrl,
        string siteAbbreviation,
        string storyUrl,
        bool isCompleted,
        IReadOnlyList<string>? category,
        IReadOnlyList<string>? characters,
        IReadOnlyList<string>? genre,
        IReadOnlyList<string>? relationships,
        string? rating,
        IReadOnlyList<string>? warnings,
        IReadOnlyList<string>? descriptionParagraphs)
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
        Category = category;
        Characters = characters;
        Genre = genre;
        Relationships = relationships;
        Rating = rating;
        Warnings = warnings;
        DescriptionParagraphs = descriptionParagraphs;
    }
}