using Interfaces;
using System.Collections.Concurrent;
namespace Services;

public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (object Value, DateTime Expiration)> _cache = new ConcurrentDictionary<string, (object, DateTime)>();

    public Task AddToCacheAsync(string key, object value, TimeSpan? expiration = null)
    {
        var expirationTime = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(1));
        _cache[key] = (value, expirationTime);

        return Task.CompletedTask;
    }

    public Task<T?> GetFromCacheAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cacheEntry))
        {
            if (DateTime.UtcNow <= cacheEntry.Expiration)
            {
                return Task.FromResult((T?)cacheEntry.Value);
            }
            else
            {            
                _cache.TryRemove(key, out var _);
            }
        }

        return Task.FromResult<T?>(default);
    }

    public Task DeleteFromCacheAsync(string key)
    {
        _cache.TryRemove(key, out var _);

        return Task.CompletedTask;
    }
}
