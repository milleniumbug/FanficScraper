namespace Common.Api;

public record StoryDetails
{
    public string Author { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public bool IsComplete { get; set; }
    public DateTime StoryUpdated { get; set; }
    public IReadOnlyList<string>? Category { get; set; }
    
    public IReadOnlyList<string>? Characters { get; set; }
    
    public IReadOnlyList<string>? Genre { get; set; }
    
    public IReadOnlyList<string>? Relationships { get; set; }
    
    public string? Rating { get; set; }
    
    public IReadOnlyList<string>? Warnings { get; set; }
    
    public IReadOnlyList<string>? DescriptionParagraphs { get; set; }
    
    public int? NumChapters { get; set; }
    
    public int? NumWords { get; set; }
}