using System.Net.Mime;
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
        
        var id = await updater.UpdateStory(new Uri(command.Url), command.Force ?? false);
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
                AuthorId = story.AuthorId,
                License = story.License,
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
                NumWords = story.StoryData?.NumWords,
                IsRemoved = story.IsRemoved,
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
        var fanFicStoryDetails = await fanFicFare.Run(new Uri(query.Url), metadataOnly: true, force: false);

        return this.Ok(new GetMetadataQueryResponse()
        {
            Id = fanFicStoryDetails.OutputFilename,
            Author = fanFicStoryDetails.Author,
            AuthorId = fanFicStoryDetails.AuthorId,
            License = fanFicStoryDetails.License,
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
            NumWords = fanFicStoryDetails.NumWords,
            SiteAbbreviation = fanFicStoryDetails.SiteAbbreviation,
        });
    }
    
    [HttpGet("Backup")]
    public async Task IssueBackup(
        [FromQuery] string? key,
        [FromQuery] bool? includeEpubs,
        [FromServices] BackupService backupService)
    {
        var request = backupService.PrepareBackup(key, includeEpubs ?? true);
        if (request == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
        
        HttpContext.Response.ContentType = request.MimeType;
        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        HttpContext.Response.Headers.ContentDisposition = new ContentDisposition
            { FileName = request.FileName, Inline = false }.ToString();
        await backupService.WriteBackup(HttpContext.Response.Body, request);
    }

    private IEnumerable<StoryDetails> Map(IEnumerable<Story> stories)
    {
        return stories.Select(story => new StoryDetails()
        {
            Author = story.AuthorName,
            AuthorId = story.AuthorId,
            License = story.License,
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