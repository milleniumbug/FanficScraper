using System.Text.Json;
using ScribbleHubFeed;

Console.WriteLine(JsonSerializer.Serialize(JsonSerializer.Serialize(new SeriesFinderSettings[]
{
    new SeriesFinderSettings()
    {
        Status = StoryStatus.All,
        SortDirection = SortOrder.Descending,
        SortBy = SortCriteria.DateAdded,
        IncludedTags = new[] { Tags.Parse("Transgender") }
    }
})));

Console.WriteLine();
