using Common.Challenges;
using FakeItEasy;
using FanficScraper.FanFicFare;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject1;

public class StoryUrlNormalizerTests
{
    private readonly StoryUrlNormalizer storyNormalizer;

    public StoryUrlNormalizerTests()
    {
        var solver = new CookieGrabberSolver(
            new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:12000"),
                Timeout = TimeSpan.FromMinutes(2),
            },
            NullLogger<CookieGrabberSolver>.Instance);

        var httpClient = new HttpClient(new ChallengeSolverHandler(solver));

        var fanficfare = A.Dummy<IFanFicFare>();

        this.storyNormalizer = new StoryUrlNormalizer(fanficfare, httpClient);
    }
    
    [Theory]
    [InlineData("http://scribblehub.com/series/759741")]
    [InlineData("http://www.scribblehub.com/series/759741")]
    [InlineData("https://www.scribblehub.com/series/759741")]
    [InlineData("https://www.scribblehub.com/series/759741/")]
    [InlineData("https://www.scribblehub.com/series/759741/?querystringgarbage")]
    [InlineData("https://www.scribblehub.com/series/759741/lol")]
    [InlineData("https://www.scribblehub.com/series/759741/lol?querystringgarbage")]
    [InlineData("https://www.scribblehub.com/series/759741/lol/")]
    [InlineData("https://www.scribblehub.com/series/759741/lol/?querystringgarbage")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story/")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story?querystringgarbage")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story?querystringgarbage#anchor")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story/?querystringgarbage")]
    [InlineData("https://www.scribblehub.com/series/759741/fragment-nyandemic-story/?querystringgarbage#anchor")]
    public async Task ScribbleHub(string input)
    {
        var expected = new Uri("https://www.scribblehub.com/series/759741/fragment-nyandemic-story/");
        var actual = await this.storyNormalizer.NormalizeUrl(new Uri(input));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("https://tgstorytime.com/viewstory.php?sid=7257&chapter=2")]
    [InlineData("https://tgstorytime.com/viewstory.php?sid=7257&ageconsent=ok&warning=4")]
    [InlineData("https://tgstorytime.com/viewstory.php?sid=7257")]
    [InlineData("http://tgstorytime.com/viewstory.php?sid=7257")]
    [InlineData("https://www.tgstorytime.com/viewstory.php?sid=7257")]
    public async Task TgStoryTime(string input)
    {
        var expected = new Uri("http://tgstorytime.com/viewstory.php?sid=7257");
        var actual = await this.storyNormalizer.NormalizeUrl(new Uri(input));

        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("http://archiveofourown.org/works/35504110?view_full_work=true")]
    [InlineData("https://archiveofourown.org/works/35504110?view_full_work=true")]
    [InlineData("https://archiveofourown.org/works/35504110/chapters/88664695")]
    public async Task ArchiveOfOurOwn(string input)
    {
        var expected = new Uri("https://archiveofourown.org/works/35504110");
        var actual = await this.storyNormalizer.NormalizeUrl(new Uri(input));

        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("https://www.royalroad.com/fiction/84820/voice-academy-diaries")]
    [InlineData("https://www.royalroad.com/fiction/84820/lol")]
    [InlineData("https://www.royalroad.com/fiction/84820/")]
    [InlineData("https://www.royalroad.com/fiction/84820")]
    public async Task RoyalRoad(string input)
    {
        var expected = new Uri("https://www.royalroad.com/fiction/84820");
        var actual = await this.storyNormalizer.NormalizeUrl(new Uri(input));

        Assert.Equal(expected, actual);
    }
}