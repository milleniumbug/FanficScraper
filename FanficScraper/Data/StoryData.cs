namespace FanficScraper.Data;

public class StoryData
{
    public int Id { get; set; }
    
    public int StoryId { get; set; }
    public Story Story { get; set; }
    
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