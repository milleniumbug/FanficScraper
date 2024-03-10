using System.Web;
using Common.Scraping;
using Common.Utils;
using ScrapySharp.Extensions;

namespace ScribbleHubFeed;

public class ScribbleHubFeed
{
    private readonly CachingScraper scraper;

    public ScribbleHubFeed(CachingScraper scraper)
    {
        this.scraper = scraper;
    }
    
    public async IAsyncEnumerable<IEnumerable<ScribbleHubStory>> ByTag(string tagName, SortCriteria? sortCriteria, SortOrder? sortOrder, StoryStatus? storyStatusFilter)
    {
        var builder = new UriBuilder($"https://www.scribblehub.com/tag/{tagName}/");
        int page = 1;

        while (true)
        {
            builder.Query = QueryStringBuilder.Create(new (string?, string?)[]
            {
                ("sort", ((int?)sortCriteria)?.ToString()),
                ("order", ((int?)sortOrder)?.ToString()),
                ("status", ((int?)storyStatusFilter)?.ToString()),
                ("pg", page.ToString())
            });
            var webDocument = await scraper.DownloadAsync(builder.Uri.ToString());
            var nodes = webDocument.Document.DocumentNode.CssSelect(".search_main_box .search_body .search_title a");
            var stories =
                nodes.Select(node => new ScribbleHubStory(
                        new Uri(node.Attributes["href"].Value),
                        node.GetInnerTextForReal()))
                    .ToList();
            
            if (stories.Count == 0)
            {
                yield break;
            }

            yield return stories;
            
            page++;
        }
    }
}