using System.Text.Json.Serialization;

namespace FanficScraper.FanFicFare;

public class FanFicFareStoryJson
{
    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("authorHTML")]
    public string AuthorHTML { get; set; }

    [JsonPropertyName("authorId")]
    public string AuthorId { get; set; }

    [JsonPropertyName("authorUrl")]
    public string AuthorUrl { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }
    
    [JsonPropertyName("characters")]
    public string? Characters { get; set; }

    [JsonPropertyName("dateCreated")]
    public string DateCreated { get; set; }

    [JsonPropertyName("datePublished")]
    public string DatePublished { get; set; }

    [JsonPropertyName("dateUpdated")]
    public string DateUpdated { get; set; }

    [JsonPropertyName("description")]
    public string DescriptionHTML { get; set; }
    
    [JsonPropertyName("genre")]
    public string? Genre { get; set; }
    
    [JsonPropertyName("output_filename")]
    public string OutputFilename { get; set; }

    [JsonPropertyName("numChapters")]
    public string NumChapters { get; set; }

    [JsonPropertyName("numWords")]
    public string NumWords { get; set; }
    
    [JsonPropertyName("rating")]
    public string? Rating { get; set; }

    [JsonPropertyName("sectionUrl")]
    public string SectionUrl { get; set; }

    [JsonPropertyName("series")]
    public string Series { get; set; }
    
    [JsonPropertyName("ships")]
    public string? Relationships { get; set; }

    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonPropertyName("siteabbrev")]
    public string SiteAbbreviation { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("storyId")]
    public string StoryId { get; set; }
    
    [JsonPropertyName("storynotes")]
    public string? StoryNotes { get; set; }

    [JsonPropertyName("storyUrl")]
    public string StoryUrl { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("warnings")]
    public string? Warnings { get; set; }
}