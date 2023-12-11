namespace Interfaces;

public interface ICacheService
{
    Task SetCacheAsync(string key, object value, TimeSpan? expiration = null);
    Task<T?> GetCacheAsync<T>(string key);
}