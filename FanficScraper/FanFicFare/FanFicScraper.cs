using System.Text.Json;
using System.Web;
using FanficScraper.Api;
using FanficScraper.Configurations;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicScraper : IFanFicFare
{
    private readonly HttpClient client;
    private readonly DataConfiguration dataConfiguration;

    public FanFicScraper(
        HttpClient client,
        IOptions<DataConfiguration> dataConfiguration)
    {
        this.client = client;
        this.dataConfiguration = dataConfiguration.Value;
    }
    
    public async Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false)
    {
        var addResponse = await this.client.PostAsJsonAsync("Api/Story", new AddStoryCommand()
        {
            Url = storyUrl,
            Passphrase = this.dataConfiguration.SecondaryFanFicScraperPassphrase
        });
        addResponse.EnsureSuccessStatusCode();

        var jsonOpts = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var addResult = await JsonSerializer.DeserializeAsync<AddStoryCommandResponse>(
            await addResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

        if (!metadataOnly)
        {
            await using var file = File.Create(Path.Combine(this.dataConfiguration.StoriesDirectory, addResult.Id));
            var storyResponse = await this.client.GetAsync($"Story/{addResult.Id}");
            storyResponse.EnsureSuccessStatusCode();
            await storyResponse.Content.CopyToAsync(file);
        }
        
        var getResponse = await this.client.GetAsync($"Api/Story/{addResult.Id}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResult = await JsonSerializer.DeserializeAsync<GetStoryQueryResponse>(
            await getResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

        return new FanFicStoryDetails(
            author: getResult.Author,
            title: getResult.Name,
            publicationDate: DateTime.MinValue,
            websiteUpdateDate: getResult.StoryUpdated,
            outputFilename: addResult.Id,
            numChapters: null,
            numWords: null,
            siteUrl: new Uri(getResult.Url).Host,
            siteAbbreviation: "",
            storyUrl: getResult.Url,
            isCompleted: getResult.IsComplete);
    }
}