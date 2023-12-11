namespace Interfaces;

public interface ICacheService
{
    Task AddToCacheAsync(string key, object value, TimeSpan? expiration = null);
    Task<T?> GetFromCacheAsync<T>(string key);
}