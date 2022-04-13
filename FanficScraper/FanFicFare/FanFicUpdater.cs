using FanficScraper.Configurations;
using FanficScraper.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicUpdater
{
    private readonly StoryContext storyContext;
    private readonly DataConfiguration dataConfiguration;

    public FanFicUpdater(
        StoryContext storyContext,
        IOptions<DataConfiguration> dataConfiguration)
    {
        this.storyContext = storyContext;
        this.dataConfiguration = dataConfiguration.Value;
    }

    public async Task UpdateOldest(TimeSpan timeSpan)
    {
        var currentDate = DateTime.UtcNow;
        var date = currentDate - timeSpan;

        var story = await this.storyContext.Stories
            .Where(story => !story.IsComplete)
            .Where(story => story.LastUpdated < date)
            .OrderBy(story => story.LastUpdated)
            .FirstOrDefaultAsync();

        if (story == null)
        {
            return;
        }
        
        var fanFicStoryDetails = await RunFanFicFare(story.StoryUrl);
        UpdateStoryEntity(story, fanFicStoryDetails, currentDate);

        await storyContext.SaveChangesAsync();
    }

    public async Task UpdateStory(string url)
    {
        var fanFicStoryDetails = await RunFanFicFare(url);
        
        var currentDate = DateTime.UtcNow;
        
        var story = await this.storyContext.Stories
            .Where(story => story.FileName == fanFicStoryDetails.OutputFilename)
            .FirstOrDefaultAsync();

        story ??= new Story();

        UpdateStoryEntity(story, fanFicStoryDetails, currentDate);

        this.storyContext.Attach(story);

        await storyContext.SaveChangesAsync();
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
    }

    private async Task<FanFicStoryDetails> RunFanFicFare(string url)
    {
        var fanFicFareSettings = new FanFicFareSettings()
        {
            IsAdult = true,
            MetadataOnly = false,
            TargetDirectory = this.dataConfiguration.StoriesDirectory
        };
        return await FanFicFare.Run(fanFicFareSettings, url);
    }
}