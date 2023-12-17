using Interfaces;

namespace Decorators
{
    public class CachingRepositoryDecorator<T> : IRepository<T> where T : class
    {
        private readonly IRepository<T> _baseRepository;
        private readonly ICacheService _cache;

        public CachingRepositoryDecorator(IRepository<T> baseRepository, ICacheService cache)
        {
            _baseRepository = baseRepository;
            _cache = cache;
        }

        public async Task AddAsync(T entity)
        {
            await _baseRepository.AddAsync(entity);

            string cacheKey = typeof(T).Name + "_list";
            Console.WriteLine($"Invalidating cache: {cacheKey}");
            await _cache.DeleteFromCacheAsync(cacheKey);
        }

        public async Task DeleteAsync(int id)
        {
            await _baseRepository.DeleteAsync(id);

            string cacheKey = $"{typeof(T).Name}_{id}";
            Console.WriteLine($"Invalidating cache: {cacheKey}");
            await _cache.DeleteFromCacheAsync(cacheKey);

            cacheKey = typeof(T).Name + "_list";
            Console.WriteLine($"Invalidating list cache: {cacheKey}");
            await _cache.DeleteFromCacheAsync(cacheKey);

        }

        public async Task<List<T>> GetAllAsync()
        {
            string cacheKey = typeof(T).Name + "_list";
            Console.WriteLine($"Checking cache for list: {cacheKey}");
            var cachedEntities = await _cache.GetFromCacheAsync<List<T>>(cacheKey);

            if (cachedEntities != null)
            {
                Console.WriteLine("Returning cached list.");
                return cachedEntities;
            }

            Console.WriteLine("Cache miss. Fetching from repository...");
            await Task.Delay(TimeSpan.FromSeconds(3));
            
            var entities = await _baseRepository.GetAllAsync();

            Console.WriteLine("Caching the fetched list...");
            await _cache.AddToCacheAsync(cacheKey, entities, TimeSpan.FromSeconds(10));

            return entities;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            string cacheKey = $"{typeof(T).Name}_{id}";
            Console.WriteLine($"Checking cache for entity ID: {id}");
            var cachedItem = await _cache.GetFromCacheAsync<T>(cacheKey);

            if (cachedItem != null)
            {
                Console.WriteLine("Returning cached entity.");
                return cachedItem;
            }

            Console.WriteLine("Cache miss. Fetching from repository...");
            await Task.Delay(TimeSpan.FromSeconds(3));

            var item = await _baseRepository.GetByIdAsync(id);            
            
            if (item != null)
            {
                Console.WriteLine("Caching the fetched entity...");
                await _cache.AddToCacheAsync(cacheKey, item, TimeSpan.FromMinutes(10));
            }

            return item;
        }

        public async Task<bool> IsEmpty()
        {
            return await _baseRepository.IsEmpty();
        }
    }
}
