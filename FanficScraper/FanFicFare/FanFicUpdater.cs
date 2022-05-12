using FanficScraper.Configurations;
using FanficScraper.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicUpdater
{
    private readonly StoryContext storyContext;
    private readonly IFanFicFare fanFicFare;

    public FanFicUpdater(
        StoryContext storyContext,
        IFanFicFare fanFicFare)
    {
        this.storyContext = storyContext;
        this.fanFicFare = fanFicFare;
    }

    public async Task<(FanFicStoryDetails? details, DateTime nextUpdateTime)> UpdateOldest(TimeSpan timeSpan)
    {
        var currentDate = DateTime.UtcNow;
        var date = currentDate - timeSpan;
        var nextUpdateTime = currentDate + timeSpan;

        var stories = await this.storyContext.Stories
            .Include(story => story.StoryData)
            .Where(story => !story.IsComplete)
            .OrderBy(story => story.LastUpdated)
            .Take(2)
            .ToListAsync();

        if (stories.Count == 0)
        {
            return (null, nextUpdateTime);
        }
        
        var oldest = stories.First();
        var secondOldest = stories.ElementAtOrDefault(1);

        if (secondOldest != null)
        {
            nextUpdateTime = secondOldest.LastUpdated + timeSpan;
        }
        
        try
        {
            var fanFicStoryDetails = await RunFanFicFare(oldest.StoryUrl, force: false);
            UpdateStoryEntity(oldest, fanFicStoryDetails, currentDate);

            return (fanFicStoryDetails, nextUpdateTime);
        }
        catch (Exception)
        {
            oldest.LastUpdated = currentDate;
            return (null, nextUpdateTime);
        }
        finally
        {
            await storyContext.SaveChangesAsync();            
        }
    }

    public async Task<string> UpdateStory(string url, bool force)
    {
        var fanFicStoryDetails = await RunFanFicFare(url, force: force);
        
        var currentDate = DateTime.UtcNow;
        
        var story = await this.storyContext.Stories
            .Include(story => story.StoryData)
            .Where(story => story.FileName == fanFicStoryDetails.OutputFilename)
            .FirstOrDefaultAsync();

        story ??= new Story()
        {
            StoryAdded = currentDate
        };

        UpdateStoryEntity(story, fanFicStoryDetails, currentDate);

        this.storyContext.Attach(story);

        await storyContext.SaveChangesAsync();

        return story.FileName;
    }

    private void UpdateStoryEntity(
        Story story,
        FanFicStoryDetails fanFicStoryDetails,
        DateTime currentDate)
    {
        story.Website = fanFicStoryDetails.SiteUrl;
        story.AuthorName = fanFicStoryDetails.Author;
        story.FileName = fanFicStoryDetails.OutputFilename;
        story.IsComplete = fanFicStoryDetails.IsCompleted;
        story.StoryName = fanFicStoryDetails.Title;
        story.LastUpdated = currentDate;
        story.StoryUpdated = fanFicStoryDetails.WebsiteUpdateDate;
        story.StoryUrl = fanFicStoryDetails.StoryUrl;

        story.StoryData ??= new StoryData();
        story.StoryData.Category = fanFicStoryDetails.Category;
        story.StoryData.Characters = fanFicStoryDetails.Characters;
        story.StoryData.Genre = fanFicStoryDetails.Genre;
        story.StoryData.Rating = fanFicStoryDetails.Rating;
        story.StoryData.Relationships = fanFicStoryDetails.Relationships;
        story.StoryData.Warnings = fanFicStoryDetails.Warnings;
        story.StoryData.DescriptionParagraphs = fanFicStoryDetails.DescriptionParagraphs;
        story.StoryData.NumChapters = fanFicStoryDetails.NumChapters;
        story.StoryData.NumWords = fanFicStoryDetails.NumWords;
    }

    private async Task<FanFicStoryDetails> RunFanFicFare(string url, bool force)
    {
        return await fanFicFare.Run(url, metadataOnly: false, force: force);
    }
}