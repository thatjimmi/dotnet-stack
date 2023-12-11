﻿using Interfaces;
using System.Collections.Concurrent;
namespace Services;

public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (object Value, DateTime Expiration)> _cache = new ConcurrentDictionary<string, (object, DateTime)>();

    public async Task AddToCacheAsync(string key, object value, TimeSpan? expiration = null)
    {
        var expirationTime = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(1));
        _cache[key] = (value, expirationTime);
        Console.WriteLine($"Added to cache: {key}");
        await Task.CompletedTask; // Simulate async work
    }

    public async Task<T?> GetFromCacheAsync<T>(string key)
    {
        Console.WriteLine($"Attempting to retrieve key: {key}");

        if (_cache.TryGetValue(key, out var cacheEntry))
        {
            if (DateTime.UtcNow <= cacheEntry.Expiration)
            {
                Console.WriteLine($"Cache hit: {key}, Type: {typeof(T)}");
                return (T)cacheEntry.Value;
            }
            else
            {
                Console.WriteLine($"Cache expired: {key}");
                _cache.TryRemove(key, out var _);
            }
        }
        else
        {
            Console.WriteLine($"Cache miss: {key}");
        }

        return default;
    }
}