using FanficScraper.Data;
using FanficScraper.FanFicFare;
using FanficScraper.Services;
using Microsoft.AspNetCore.Mvc;

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
                StoryUpdated = story.StoryUpdated
            });
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
            Url = fanFicStoryDetails.StoryUrl
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
            StoryUpdated = story.StoryUpdated
        });
    }
}