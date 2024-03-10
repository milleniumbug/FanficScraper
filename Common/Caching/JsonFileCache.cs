using System.Text.Json;

namespace Common.Caching;

#nullable enable

public class JsonFileCache : ICache<string, IReadOnlyList<string>>, IDisposable, IAsyncDisposable
{
    private readonly string filePath;
    private Dictionary<string, IReadOnlyList<string>> dataImpl;
    private IDictionary<string, IReadOnlyList<string>> Data => dataImpl;

    private JsonFileCache(string path)
    {
        filePath = path;
        dataImpl = new Dictionary<string, IReadOnlyList<string>>();
    }

    public static JsonFileCache Create(string path)
    {
        var c = new JsonFileCache(path);
        c.Load();
        return c;
    }
    
    public static async Task<JsonFileCache> CreateAsync(string path)
    {
        var c = new JsonFileCache(path);
        await c.LoadAsync();
        return c;
    }

    public void Load()
    {
        try
        {
            dataImpl = JsonSerializer.Deserialize<Dictionary<string, IReadOnlyList<string>>>(
                File.ReadAllText(filePath)) ?? throw new InvalidDataException();
        }
        catch (FileNotFoundException)
        {
            // do nothing
        }
    }
    
    public async Task LoadAsync()
    {
        try
        {
            dataImpl = JsonSerializer.Deserialize<Dictionary<string, IReadOnlyList<string>>>(
                await File.ReadAllTextAsync(filePath)) ?? throw new InvalidDataException();
        }
        catch (FileNotFoundException)
        {
            // do nothing
        }
    }

    public void Save()
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(dataImpl));
    }

    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(dataImpl));
    }

    public void Dispose()
    {
        Save();
    }
    
    public async ValueTask DisposeAsync()
    {
        await SaveAsync();
    }

    public Task<IReadOnlyList<string>?> Get(string key)
    {
        return Task.FromResult(dataImpl.GetValueOrDefault(key));
    }

    public Task Set(string key, IReadOnlyList<string> value)
    {
        dataImpl[key] = value;
        return Task.CompletedTask;
    }

    public async Task Clear()
    {
        dataImpl = new Dictionary<string, IReadOnlyList<string>>();
        await ForceSave();
    }

    public async Task ForceSave()
    {
        await SaveAsync();
    }
}