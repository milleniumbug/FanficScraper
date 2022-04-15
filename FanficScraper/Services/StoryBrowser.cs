using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.EntityFrameworkCore;

namespace FanficScraper.Services;

public class StoryBrowser
{
    private readonly StoryContext storyContext;
    private readonly IFanFicFare fanFicFare;

    public StoryBrowser(
        StoryContext storyContext,
        IFanFicFare fanFicFare)
    {
        this.storyContext = storyContext;
        this.fanFicFare = fanFicFare;
    }
    
    public async Task<Story?> FindByUrl(string url)
    {
        var fanFicFareSettings = new FanFicFareSettings()
        {
            IsAdult = true,
        };
        var fanFicStoryDetails = await fanFicFare.Run(url, metadataOnly: true);

        return await this.storyContext.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(story => story.StoryUrl == url);
    }
    
    public async Task<Story?> FindById(string id)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(story => story.FileName == id);
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