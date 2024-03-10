using Common.Api;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;
using DownloadJobStatus = Common.Api.DownloadJobStatus;

namespace FanficScraper.Api;

[ApiController]
[Route("Api")]
public class ApiController : Controller
{
    [HttpPost("Story")]
    public async Task<IActionResult> AddStory(
        [FromBody] AddStoryCommand command,
        [FromServices] FanFicUpdater updater,
        [FromServices] UserManager userManager)
    {
        if (await userManager.IsAuthorized(command.Passphrase) != AuthorizationResult.Success)
        {
            return this.Unauthorized();
        }
        
        var id = await updater.UpdateStory(command.Url, command.Force ?? false);
        return this.Ok(new AddStoryCommandResponse()
        {
            Id = id
        });
    }

    [HttpGet("Story/{id}")]
    public async Task<IActionResult> GetStory(
        [FromRoute] string id,
        [FromServices] StoryBrowser browser)
    {
        var story = await browser.FindById(id);
        if (story != null)
        {
            return this.Ok(new GetStoryQueryResponse()
            {
                Author = story.AuthorName,
                Name = story.StoryName,
                Url = story.StoryUrl,
                IsComplete = story.IsComplete,
                StoryUpdated = story.StoryUpdated,
                Category = story.StoryData?.Category,
                Characters = story.StoryData?.Characters,
                Genre = story.StoryData?.Genre,
                Relationships = story.StoryData?.Relationships,
                Rating = story.StoryData?.Rating,
                Warnings = story.StoryData?.Warnings,
                DescriptionParagraphs = story.StoryData?.DescriptionParagraphs,
                NumChapters = story.StoryData?.NumChapters,
                NumWords = story.StoryData?.NumWords
            });
        }
        else
        {
            return this.NotFound();
        }
    }
    
    [HttpPost("StoryAsync")]
    public async Task<IActionResult> AddStoryAsync(
        [FromBody] AddStoryAsyncCommand command,
        [FromServices] FanFicUpdater updater,
        [FromServices] UserManager userManager)
    {
        if (await userManager.IsAuthorized(command.Passphrase) != AuthorizationResult.Success)
        {
            return this.Unauthorized();
        }
        
        var id = await updater.ScheduleUpdateStory(command.Url, command.Force ?? false);
        return this.Ok(new AddStoryAsyncCommandResponse()
        {
            JobId = id
        });
    }
    
    [HttpGet("StoryAsync/{id}")]
    public async Task<IActionResult> GetStoryAsync(
        [FromRoute] Guid id,
        [FromServices] FanFicUpdater updater)
    {
        var jobDetails = await updater.GetScheduledJobDetails(id);
        if (jobDetails != null)
        {
            return this.Ok(
                new GetStoryAsyncQueryResponse(
                    jobId: id,
                    storyId: jobDetails.StoryId,
                    status: (DownloadJobStatus)jobDetails.Status,
                    url: jobDetails.Url));
        }
        else
        {
            return this.NotFound();
        }
    }
    
    [HttpGet("StorySearch")]
    public async Task<IActionResult> FindStories(
        [FromQuery] string? name,
        [FromServices] StoryBrowser browser)
    {
        if (name != null)
        {
            var stories = await browser.FindByName(name);
            return this.Ok(new FindStoriesQueryResponse
            {
                Results = this.Map(stories)
            });
        }

        return this.NotFound();
    }
    
    [HttpPost("Metadata")]
    public async Task<IActionResult> GetMetadata(
        [FromBody] GetMetadataQuery query,
        [FromServices] IFanFicFare fanFicFare)
    {
        var fanFicStoryDetails = await fanFicFare.Run(query.Url, metadataOnly: true, force: false);

        return this.Ok(new GetMetadataQueryResponse()
        {
            Id = fanFicStoryDetails.OutputFilename,
            Author = fanFicStoryDetails.Author,
            Name = fanFicStoryDetails.Title,
            IsComplete = fanFicStoryDetails.IsCompleted,
            StoryUpdated = fanFicStoryDetails.WebsiteUpdateDate,
            Url = fanFicStoryDetails.StoryUrl,
            Category = fanFicStoryDetails.Category,
            Characters = fanFicStoryDetails.Characters,
            Genre = fanFicStoryDetails.Genre,
            Relationships = fanFicStoryDetails.Relationships,
            Rating = fanFicStoryDetails.Rating,
            Warnings = fanFicStoryDetails.Warnings,
            DescriptionParagraphs = fanFicStoryDetails.DescriptionParagraphs,
            NumChapters = fanFicStoryDetails.NumChapters,
            NumWords = fanFicStoryDetails.NumWords
        });
    }

    private IEnumerable<StoryDetails> Map(IEnumerable<Story> stories)
    {
        return stories.Select(story => new StoryDetails()
        {
            Author = story.AuthorName,
            Name = story.StoryName,
            Url = story.StoryUrl,
            IsComplete = story.IsComplete,
            StoryUpdated = story.StoryUpdated,
            Category = story.StoryData?.Category,
            Characters = story.StoryData?.Characters,
            Genre = story.StoryData?.Genre,
            Relationships = story.StoryData?.Relationships,
            Rating = story.StoryData?.Rating,
            Warnings = story.StoryData?.Warnings,
            DescriptionParagraphs = story.StoryData?.DescriptionParagraphs,
            NumChapters = story.StoryData?.NumChapters,
            NumWords = story.StoryData?.NumWords
        });
    }
}