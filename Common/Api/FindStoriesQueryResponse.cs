namespace Common.Api;

public class FindStoriesQueryResponse
{
    public IEnumerable<StoryDetails> Results { get; init; }
}