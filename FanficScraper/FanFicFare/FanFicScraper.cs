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
    
    public async Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false, bool force = false)
    {
        var jsonOpts = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        try
        {
            var addResponse = await this.client.PostAsJsonAsync("Api/Story", new AddStoryCommand()
            {
                Url = storyUrl,
                Passphrase = this.dataConfiguration.SecondaryFanFicScraperPassphrase,
                Force = force
            });
            addResponse.EnsureSuccessStatusCode();

            var addResult = await JsonSerializer.DeserializeAsync<AddStoryCommandResponse>(
                await addResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

            return await FanFicStoryDetails(addResult.Id);
        }
        catch (Exception ex)
        {
            var metadataResponse = await this.client.PostAsJsonAsync("Api/Metadata", new GetMetadataQuery()
            {
                Url = storyUrl
            });
            metadataResponse.EnsureSuccessStatusCode();
            
            var metadataResult = await JsonSerializer.DeserializeAsync<GetMetadataQueryResponse>(
                await metadataResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();
            
            return await FanFicStoryDetails(metadataResult.Id);
        }

        async Task<FanFicStoryDetails> FanFicStoryDetails(string id)
        {
            if (!metadataOnly)
            {
                await using var file = File.Create(Path.Combine(this.dataConfiguration.StoriesDirectory, id));
                var storyResponse = await this.client.GetAsync($"Story/{id}");
                storyResponse.EnsureSuccessStatusCode();
                await storyResponse.Content.CopyToAsync(file);
            }

            var getResponse = await this.client.GetAsync($"Api/Story/{id}");
            getResponse.EnsureSuccessStatusCode();

            var getResult = await JsonSerializer.DeserializeAsync<GetStoryQueryResponse>(
                await getResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

            return new FanFicStoryDetails(
                author: getResult.Author,
                title: getResult.Name,
                publicationDate: DateTime.MinValue,
                websiteUpdateDate: getResult.StoryUpdated,
                outputFilename: id,
                numChapters: null,
                numWords: null,
                siteUrl: new Uri(getResult.Url).Host,
                siteAbbreviation: "",
                storyUrl: getResult.Url,
                isCompleted: getResult.IsComplete);
        }
    }
}