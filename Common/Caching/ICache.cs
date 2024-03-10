#nullable enable
namespace Common.Caching;

public interface ICache<in TKey, TValue> : IAsyncDisposable
    where TKey : notnull
    where TValue : notnull
{
    Task<TValue?> Get(TKey key);

    Task Set(TKey key, TValue value);

    Task Clear();

    Task ForceSave();
}