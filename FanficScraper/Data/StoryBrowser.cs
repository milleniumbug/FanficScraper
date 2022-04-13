using FanficScraper.FanFicFare;
using Microsoft.EntityFrameworkCore;

namespace FanficScraper.Data;

public class StoryBrowser
{
    private readonly StoryContext storyContext;

    public StoryBrowser(
        StoryContext storyContext)
    {
        this.storyContext = storyContext;
    }
    
    public async Task<Story?> FindByUrl(string url)
    {
        var fanFicFareSettings = new FanFicFareSettings()
        {
            IsAdult = true,
            MetadataOnly = true,
        };
        var fanFicStoryDetails = await FanFicFare.FanFicFare.Run(fanFicFareSettings, url);

        return await this.storyContext.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(story => story.StoryUrl == url);
    }
    
    public async Task<IEnumerable<Story>> FindByName(string name)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .Where(story => story.StoryName.Contains(name))
            .Take(10)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Story>> FindLastUpdated(int count)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .OrderByDescending(story => story.StoryUpdated)
            .Take(count)
            .ToListAsync();
    }
}