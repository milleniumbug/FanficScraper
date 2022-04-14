using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.AspNetCore.Mvc;

namespace FanficScraper.Api;

[ApiController]
[Route("Api")]
public class ApiController : Controller
{
    [HttpPost("Story")]
    public async Task<IActionResult> AddStory(
        [FromBody] AddStoryCommand command,
        [FromServices] FanFicUpdater updater)
    {
        var id = await updater.UpdateStory(command.Url);
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