using System.Text.Json;
using System.Web;
using FanficScraper.Api;
using FanficScraper.Configurations;
using FanficScraper.Data;
using Microsoft.Extensions.Options;

namespace FanficScraper.FanFicFare;

public class FanFicScraper : IFanFicFare
{
    private readonly HttpClient client;
    private readonly ILogger<FanFicScraper> logger;
    private readonly DataConfiguration dataConfiguration;

    public FanFicScraper(
        HttpClient client,
        IOptions<DataConfiguration> dataConfiguration,
        ILogger<FanFicScraper> logger)
    {
        this.client = client;
        this.logger = logger;
        this.dataConfiguration = dataConfiguration.Value;
    }
    
    public async Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false, bool force = false)
    {
        var jsonOpts = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        using var addResponse = await this.client.PostAsJsonAsync("Api/StoryAsync", new AddStoryAsyncCommand()
        {
            Url = storyUrl,
            Passphrase = this.dataConfiguration.SecondaryFanFicScraperPassphrase,
            Force = force
        });
        addResponse.EnsureSuccessStatusCode();

        var addResult = await JsonSerializer.DeserializeAsync<AddStoryAsyncCommandResponse>(
            await addResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

        this.logger.LogInformation("scheduled a download job to the secondary FanFicScraper instance");
        
        GetStoryAsyncQueryResponse queryResult;
        do
        {
            this.logger.LogInformation("periodic secondary FanFicScraper check");
            using var queryResponse = await this.client.GetAsync($"Api/StoryAsync/{addResult.JobId}");

            queryResponse.EnsureSuccessStatusCode();
            queryResult = await JsonSerializer.DeserializeAsync<GetStoryAsyncQueryResponse>(
                await queryResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

            await Task.Delay(5000);
        } while (queryResult.Status == DownloadJobStatus.NotYetStarted || queryResult.Status == DownloadJobStatus.Started);

        if (queryResult.Status == DownloadJobStatus.Failed || queryResult.StoryId == null)
        {
            this.logger.LogError("secondary FanFicScraper reported failure");
            throw new HttpRequestException("story download failed");
        }
        

        return await FanFicStoryDetails(queryResult.StoryId);

        async Task<FanFicStoryDetails> FanFicStoryDetails(string id)
        {
            if (!metadataOnly)
            {
                await using var file = File.Create(Path.Combine(this.dataConfiguration.StoriesDirectory, id));
                using var storyResponse = await this.client.GetAsync($"Story/{id}");
                storyResponse.EnsureSuccessStatusCode();
                await storyResponse.Content.CopyToAsync(file);
                this.logger.LogInformation("downloaded story from secondary FanFicScraper instance");
            }

            using var getResponse = await this.client.GetAsync($"Api/Story/{id}");
            getResponse.EnsureSuccessStatusCode();

            var getResult = await JsonSerializer.DeserializeAsync<GetStoryQueryResponse>(
                await getResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

            this.logger.LogInformation("downloaded metadata from secondary FanFicScraper instance");
            
            return new FanFicStoryDetails(
                author: getResult.Author,
                title: getResult.Name,
                publicationDate: DateTime.MinValue,
                websiteUpdateDate: getResult.StoryUpdated,
                outputFilename: id,
                numChapters: getResult.NumChapters,
                numWords: getResult.NumWords,
                siteUrl: new Uri(getResult.Url).Host,
                siteAbbreviation: "",
                storyUrl: getResult.Url,
                isCompleted: getResult.IsComplete,
                category: getResult.Category,
                characters: getResult.Characters,
                genre: getResult.Genre,
                relationships: getResult.Relationships,
                rating: getResult.Rating,
                warnings: getResult.Warnings,
                descriptionParagraphs: getResult.DescriptionParagraphs);
        }
    }
}