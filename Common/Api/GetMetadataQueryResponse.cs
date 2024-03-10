namespace Common.Api;

public record GetMetadataQueryResponse : StoryDetails
{
    public string Id { get; set; }
}