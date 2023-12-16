using Interfaces;
using Models;

namespace Decorators
{
    public class CachingProductRepositoryDecorator : IProductRepository
    {
        private readonly IProductRepository _baseRepository;
        private readonly ICacheService _cache;

        public CachingProductRepositoryDecorator(IProductRepository baseRepository, ICacheService cache)
        {
            _baseRepository = baseRepository;
            _cache = cache;
        }

        public async Task AddAsync(Product product)
        {
            await _baseRepository.AddAsync(product);           

            // Invalidate the products list cache as the list has changed
            await _cache.DeleteFromCacheAsync("products");
        }

        public async Task DeleteAsync(int id)
        {
            await _baseRepository.DeleteAsync(id);

            // Invalidate the products list and product_id cache
            await _cache.DeleteFromCacheAsync("products");
            await _cache.DeleteFromCacheAsync($"product_{id}");
        }

        public async Task<List<Product>> GetAllAsync()
        {
            string cacheKey = "products";
            var cachedProducts = await _cache.GetFromCacheAsync<List<Product>>(cacheKey);

            if (cachedProducts != null)
            {
                return cachedProducts;
            }

            await Task.Delay(TimeSpan.FromSeconds(3));

            var products = await _baseRepository.GetAllAsync();
            
            await _cache.AddToCacheAsync(cacheKey, products, TimeSpan.FromSeconds(10));

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            string cacheKey = $"product_{id}";
            var cachedProduct = await _cache.GetFromCacheAsync<Product>(cacheKey);

            if (cachedProduct != null)
            {
                return cachedProduct;
            }

            var product = await _baseRepository.GetByIdAsync(id);

            await Task.Delay(TimeSpan.FromSeconds(3));

            if (product != null)
            {
                await _cache.AddToCacheAsync(cacheKey, product, TimeSpan.FromSeconds(10));
            }

            return product;
        }
    }
}
