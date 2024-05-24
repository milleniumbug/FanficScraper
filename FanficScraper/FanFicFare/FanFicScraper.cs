using System.Text.Json;
using Common;
using Common.Api;
using FanficScraper.Configurations;
using Microsoft.Extensions.Options;
using DownloadJobStatus = Common.Api.DownloadJobStatus;

namespace FanficScraper.FanFicFare;

public class FanFicScraper : IFanFicFare
{
    private readonly FanFicScraperClient fanFicScraperClient;
    private readonly ILogger<FanFicScraper> logger;
    private readonly DataConfiguration dataConfiguration;

    public FanFicScraper(
        FanFicScraperClient fanFicScraperClient,
        IOptions<DataConfiguration> dataConfiguration,
        ILogger<FanFicScraper> logger)
    {
        this.fanFicScraperClient = fanFicScraperClient;
        this.logger = logger;
        this.dataConfiguration = dataConfiguration.Value;
    }
    
    public async Task<FanFicStoryDetails> Run(Uri storyUrl, bool metadataOnly = false, bool force = false)
    {
        var addResult = await fanFicScraperClient.AddStoryAsync(
            new AddStoryAsyncCommand()
            {
                Url = storyUrl.ToString(),
                Passphrase = this.dataConfiguration.SecondaryFanFicScraperPassphrase
                             ?? throw new InvalidOperationException("missing secondary instance passphrase"),
                Force = force
            });

        this.logger.LogInformation("scheduled a download job to the secondary FanFicScraper instance");
        
        GetStoryAsyncQueryResponse queryResult;
        do
        {
            this.logger.LogInformation("periodic secondary FanFicScraper check");
            queryResult = await fanFicScraperClient.CheckAdd(addResult);

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
                await using var story = await fanFicScraperClient.DownloadStoryById(id);
                await story.CopyToAsync(file);
                this.logger.LogInformation("downloaded story from secondary FanFicScraper instance");
            }

            var getResult = await this.fanFicScraperClient.GetStoryById(id)
                ?? throw new InvalidDataException();

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