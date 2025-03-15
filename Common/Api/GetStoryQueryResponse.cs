namespace Common.Api;

public record GetStoryQueryResponse : StoryDetails
{
    public bool? IsRemoved { get; set; }
    
    public bool IsArchived { get; set; }
}