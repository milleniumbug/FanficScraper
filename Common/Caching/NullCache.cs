#nullable enable
namespace Common.Caching;

public class NullCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task<TValue?> Get(TKey key)
    {
        return Task.FromResult<TValue?>(default);
    }

    public Task Set(TKey key, TValue value)
    {
        return Task.CompletedTask;
    }

    public Task Clear()
    {
        return Task.CompletedTask;
    }

    public Task ForceSave()
    {
        return Task.CompletedTask;
    }
}