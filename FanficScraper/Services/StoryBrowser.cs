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
        var fanFicStoryDetails = await fanFicFare.Run(url, metadataOnly: true, force: false);

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
        var escapedName = name
            .Replace("!", "!!")
            .Replace("%", "!%")
            .Replace("_", "!_");
        return await this.storyContext.Stories
            .AsNoTracking()
            .Where(story => EF.Functions.Like(story.StoryName, $"%{escapedName}%", "!"))
            .Take(10)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Story>> FindLastUpdated(int count)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .OrderByDescending(story => story.StoryUpdated)
            .ThenByDescending(story => story.LastUpdated)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Story>> FindRecentlyAdded(int count)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .Where(story => story.StoryAdded != null)
            .OrderByDescending(story => story.StoryAdded)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Story>> FindRecentlyAdded(DateTime lastAdded)
    {
        return await this.storyContext.Stories
            .AsNoTracking()
            .Where(story => story.StoryAdded != null)
            .Where(story => lastAdded < story.StoryAdded)
            .OrderByDescending(story => story.StoryAdded)
            .ToListAsync();
    }
}