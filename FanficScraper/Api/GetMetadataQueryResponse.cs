namespace FanficScraper.Api;

public record GetMetadataQueryResponse : StoryDetails
{
    public string Id { get; set; }
}