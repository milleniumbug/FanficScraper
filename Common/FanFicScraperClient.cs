using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using Common.Api;
using Common.Utils;

namespace Common;

public class FanFicScraperClient
{
    private readonly HttpClient client;
    
    private JsonSerializerOptions jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public FanFicScraperClient(
        HttpClient client)
    {
        this.client = client;
    }

    public async Task<AddStoryAsyncCommandResponse> AddStoryAsync(AddStoryAsyncCommand command)
    {
        var url = "Api/StoryAsync";
        
        var rawRequest = JsonSerializer.Serialize(command, jsonOpts);
        using var response = await this.client.PostAsync(url,
            new StringContent(
                rawRequest,
                Encoding.UTF8,
                "application/json"));
        response.EnsureSuccessStatusCode();
        var rawResponse = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<AddStoryAsyncCommandResponse>(
            rawResponse, jsonOpts) ?? throw new JsonException();

        return result;
    }
    
    public async Task<GetStoryAsyncQueryResponse> CheckAdd(AddStoryAsyncCommandResponse downloadJob)
    {
        var url = $"Api/StoryAsync/{HttpUtility.UrlEncode(downloadJob.JobId.ToString())}";
        
        using var response = await this.client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var rawResponse = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<GetStoryAsyncQueryResponse>(
            rawResponse, jsonOpts) ?? throw new JsonException();

        return result;
    }
    
    public async Task<FindStoriesQueryResponse> FindStories(string name)
    {
        var url = $"Api/StorySearch?name={HttpUtility.UrlEncode(name)}";
        
        using var response = await this.client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var rawResponse = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<FindStoriesQueryResponse>(
            rawResponse, jsonOpts) ?? throw new JsonException();

        return result;
    }
    
    public async Task<GetStoryAsyncQueryResponse> AwaitAdd(AddStoryAsyncCommandResponse downloadJob)
    {
        GetStoryAsyncQueryResponse queryResult;
        do
        {
            queryResult = await CheckAdd(downloadJob);
            await Task.Delay(5000);
        } while (queryResult.Status == DownloadJobStatus.NotYetStarted || queryResult.Status == DownloadJobStatus.Started);

        return queryResult;
    }

    public async Task<Stream> DownloadStoryById(string id)
    {
        var storyResponse = await this.client.GetAsync($"Story/{id}");
        storyResponse.EnsureSuccessStatusCode();
        return await storyResponse.Content.ReadAsStreamAsync();
    }

    public async Task<GetStoryQueryResponse?> GetStoryById(string id)
    {
        using var getResponse = await this.client.GetAsync($"Api/Story/{id}");
        if (getResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        getResponse.EnsureSuccessStatusCode();

        var getResult = await JsonSerializer.DeserializeAsync<GetStoryQueryResponse>(
            await getResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

        return getResult;
    }
    
    public async Task<GetMetadataQueryResponse> GetMetadata(GetMetadataQuery query)
    {
        var url = "Api/Metadata";
        
        var rawRequest = JsonSerializer.Serialize(query, jsonOpts);
        using var response = await this.client.PostAsync(url,
            new StringContent(
                rawRequest,
                Encoding.UTF8,
                "application/json"));
        response.EnsureSuccessStatusCode();
        var rawResponse = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<GetMetadataQueryResponse>(
            rawResponse, jsonOpts) ?? throw new JsonException();

        return result;
    }

    public async Task<(HttpStatusCode code, Stream? backupStream)> Backup(string? key = null, bool? includeEpubs = null)
    {
        var url = "Api/Backup";
        var urlBuilder = new UriBuilder(new Uri(this.client.BaseAddress!, new Uri(url, UriKind.Relative)))
        {
            Query = QueryStringBuilder.Create([
                ("key", key),
                ("includeEpubs", null)
            ]) 
        };
        var response = await this.client.GetAsync(urlBuilder.ToString());
        return (
            response.StatusCode,
            response.IsSuccessStatusCode
                ? await response.Content.ReadAsStreamAsync()
                : null);
    }
}