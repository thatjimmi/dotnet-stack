/*
 * 
 * Afkobling af Redis Logik
 * At have Redis-logikken i en særskilt klasse RedisService,
 * der implementerer ICacheService-interfacet, er en fremragende praksis. 
 * Dette gør det nemmere at udskifte Redis med en anden cache-løsning i fremtiden, 
 * hvis det er nødvendigt.
 * 
 *
*/ 

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Interfaces;

namespace Services

{
	public class RedisCacheService : ICacheService
	{
		private readonly IDistributedCache _cache;

        /*
		 * Dependency Injection:
		 * The constructor takes IDistributedCache as a dependency, 
		 * which is an abstraction over distributed cache stores like Redis. 
		 * This allows for loose coupling. 
		*/
        public RedisCacheService(IDistributedCache cache)
		{
			_cache = cache;
		}

		public async Task AddToCacheAsync(string key, object value, TimeSpan? expiration = null)
		{
			var options = new DistributedCacheEntryOptions
			{
				 AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(1),
				//SlidingExpiration = expiration ?? TimeSpan.FromMinutes(1)
			};

			var jsonData = JsonSerializer.Serialize(value);
			await _cache.SetStringAsync(key, jsonData, options);
		}

        public async Task DeleteFromCacheAsync(string key)
        {
			await _cache.RemoveAsync(key);
        }

        public async Task<T?> GetFromCacheAsync<T>(string key)
		{
			var jsonData = await _cache.GetStringAsync(key);

			return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
		}
	}
}

