using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using Common.Api;

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

    public async Task<GetStoryQueryResponse> GetStoryById(string id)
    {
        using var getResponse = await this.client.GetAsync($"Api/Story/{id}");
        getResponse.EnsureSuccessStatusCode();

        var getResult = await JsonSerializer.DeserializeAsync<GetStoryQueryResponse>(
            await getResponse.Content.ReadAsStreamAsync(), jsonOpts) ?? throw new JsonException();

        return getResult;
    }
}