using System.Globalization;

namespace ScribbleHubFeed;

public record SeriesFinderSettings
{
    public Alternative? GenreInclusion { get; init; }
    
    public IEnumerable<Genre>? IncludedGenres { get; init; }
    
    public IEnumerable<Genre>? ExcludedGenres { get; init; }
    
    public IEnumerable<Fandom>? IncludedFandoms { get; init; }
    
    public Alternative? TagInclusion { get; init; }
    
    public IEnumerable<Tag>? IncludedTags { get; init; }
    
    public IEnumerable<Tag>? ExcludedTags { get; init; }
    
    public Alternative? ContentWarningSelection { get; init; }
    
    public IEnumerable<Tag>? IncludedContentWarnings { get; init; }
    
    public IEnumerable<Tag>? ExcludedContentWarnings { get; init; }

    public SortOrder? SortDirection { get; init; } = SortOrder.Descending;

    public SortCriteria? SortBy { get; init; } = SortCriteria.Popularity;

    public StoryStatus? Status { get; init; } = StoryStatus.All;
    
    public string? TitleContains { get; init; }
    
    public int? MinimalChaptersCount { get; init; }
    
    public int? MaximalChaptersCount { get; init; }
    
    public int? MinimalChaptersPerWeek { get; init; }
    
    public int? MaximalChaptersPerWeek { get; init; }
    
    public int? MinimalFavorites { get; init; }
    
    public int? MaximalFavorites { get; init; }
    
    public int? MinimalRatings { get; init; }

    public int? MaximalRatings { get; init; }
    
    public int? MinimalNumberOfRatings { get; init; }
    
    public int? MaximalNumberOfRatings { get; init; }
    
    public int? MinimalReaders { get; init; }

    public int? MaximalReaders { get; init; }
    
    public int? MinimalReviews { get; init; }

    public int? MaximalReviews { get; init; }
    
    public DateOnly? LastUpdateFrom { get; init; }
    
    public DateOnly? LastUpdateTo { get; init; }
    
    public int? MinimalPages { get; init; }

    public int? MaximalPages { get; init; }

    public int? MinimalPageViews { get; init; }

    public int? MaximalPageViews { get; init; }

    public int? MinimalTotalWords { get; init; }

    public int? MaximalTotalWords { get; init; }
    
    public IEnumerable<(string? key, string? value)> ToQueryParams()
    {
        yield return ("mgi", ToString(GenreInclusion));
        yield return ("gi", EnumsToString(IncludedGenres));
        yield return ("ge", EnumsToString(ExcludedGenres));
        
        yield return ("fdi", EnumsToString(IncludedFandoms));
        
        yield return ("mtgi", ToString(TagInclusion));
        yield return ("tgi", EnumsToString(IncludedTags));
        yield return ("tge", EnumsToString(ExcludedTags));
        
        yield return ("mct", ToString(ContentWarningSelection));
        yield return ("cti", EnumsToString(IncludedContentWarnings));
        yield return ("cte", EnumsToString(ExcludedContentWarnings));
        
        yield return ("cp", ToString(Status));
        yield return ("order", ToString(SortDirection));
        yield return ("sort", ToString(SortBy));
        
        yield return ("sh", TitleContains);
        yield return ("mnc", MinimalChaptersCount?.ToString());
        yield return ("mxc", MaximalChaptersCount?.ToString());
        yield return ("mnfv", MinimalFavorites?.ToString());
        yield return ("mxfv", MaximalFavorites?.ToString());
        yield return ("mnrt", MinimalRatings?.ToString());
        yield return ("mxrt", MaximalRatings?.ToString());
        yield return ("mnnrt", MinimalNumberOfRatings?.ToString());
        yield return ("mxnrt", MaximalNumberOfRatings?.ToString());
        yield return ("mnrpw", MinimalChaptersPerWeek?.ToString());
        yield return ("mxrpw", MaximalChaptersPerWeek?.ToString());
        yield return ("mnrr", MinimalReaders?.ToString());
        yield return ("mxrr", MaximalReaders?.ToString());
        yield return ("mnrv", MinimalReviews?.ToString());
        yield return ("mxrv", MaximalReviews?.ToString());
        
        yield return ("mnlcd", LastUpdateFrom?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
        yield return ("mxlcd", LastUpdateTo?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
        yield return ("mnpv", (MinimalPageViews / 1000)?.ToString());
        yield return ("mxpv", (MaximalPageViews / 1000)?.ToString());
        yield return ("mnps", MinimalPages?.ToString());
        yield return ("mxps", MaximalPages?.ToString());
        yield return ("mntw", (MinimalTotalWords / 1000)?.ToString());
        yield return ("mxtw", (MaximalTotalWords / 1000)?.ToString());
    }

    private string? ToString(SortCriteria? sortCriteria)
    {
        return sortCriteria switch
        {
            SortCriteria.Chapters => "chapters",
            SortCriteria.ChaptersPerWeek => "frequency",
            SortCriteria.DateAdded => "dateadded",
            SortCriteria.Favorites => "favorites",
            SortCriteria.LastUpdate => "lastchpdate",
            SortCriteria.Pages => "pages",
            SortCriteria.Popularity => "pageviews",
            SortCriteria.Ratings => "numofrate",
            SortCriteria.Readers => "readers",
            SortCriteria.Reviews => "reviews",
            SortCriteria.WordCount => "totalwords",
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string? ToString(StoryStatus? storyStatus)
    {
        return storyStatus switch
        {
            StoryStatus.All => null,
            StoryStatus.Completed => "completed",
            StoryStatus.Ongoing => "ongoing",
            StoryStatus.Hiatus => "hiatus",
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string? ToString(SortOrder? sortDirection)
    {
        return sortDirection switch
        {
            SortOrder.Ascending => "asc",
            SortOrder.Descending => "desc",
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string? ToString(Alternative? alternative)
    {
        return alternative switch
        {
            Alternative.And => "and",
            Alternative.Or => "or",
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string? EnumsToString<T>(IEnumerable<T>? enums)
    {
        if (enums == null)
        {
            return null;
        }

        return string.Join(",", enums.Select(e => Convert.ToInt32(e).ToString()));
    }
}