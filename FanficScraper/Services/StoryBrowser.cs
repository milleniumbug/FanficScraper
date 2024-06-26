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
    
    public async Task<Story?> FindByUrl(Uri url)
    {
        var fanFicStoryDetails = await fanFicFare.Run(url, metadataOnly: true, force: false);

        return await this.storyContext.Stories
            .Include(story => story.StoryData)
            .AsNoTracking()
            .FirstOrDefaultAsync(story => story.StoryUrl == url.ToString());
    }
    
    public async Task<Story?> FindById(string id)
    {
        return await this.storyContext.Stories
            .Include(story => story.StoryData)
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
            .Include(story => story.StoryData)
            .AsNoTracking()
            .Where(story => EF.Functions.Like(story.StoryName, $"%{escapedName}%", "!"))
            .Take(10)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Story>> FindLastUpdated(int count)
    {
        var stories = await this.storyContext.Stories
            .Include(story => story.StoryData)
            .AsNoTracking()
            .OrderByDescending(story => story.StoryUpdated)
            .ThenByDescending(story => story.LastUpdated)
            .Take(count)
            .ToListAsync();

        return stories;
    }

    public async Task<IEnumerable<Story>> FindRecentlyAdded(int count)
    {
        return await this.storyContext.Stories
            .Include(story => story.StoryData)
            .AsNoTracking()
            .Where(story => story.StoryAdded != null)
            .OrderByDescending(story => story.StoryAdded)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Story>> FindRecentlyAdded(DateTime lastAdded)
    {
        return await this.storyContext.Stories
            .Include(story => story.StoryData)
            .AsNoTracking()
            .Where(story => story.StoryAdded != null)
            .Where(story => lastAdded < story.StoryAdded)
            .OrderByDescending(story => story.StoryAdded)
            .ToListAsync();
    }

    public async Task<IEnumerable<Story>> FindByAuthor(string authorName)
    {
        return await this.storyContext.Stories
            .Include(story => story.StoryData)
            .AsNoTracking()
            .Where(story => story.AuthorName == authorName)
            .OrderByDescending(story => story.StoryAdded)
            .ThenByDescending(story => story.LastUpdated)
            .ToListAsync();
    }
}