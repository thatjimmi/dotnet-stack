using Interfaces;
using Models;

namespace Repositories
{
	public class FakeProductRepository : IRepository<Product>
	{
        private List<Product> _products = new List<Product>();

        public Task AddAsync(Product product)
        {
            if (_products.Count == 0)
                product.ProductID = 1;
            else
                product.ProductID = _products.Max(p => p.ProductID) + 1;    
            _products.Add(product);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var productToRemove = _products.FirstOrDefault(p => p.ProductID == id);
            if (productToRemove != null)
            {
                _products.Remove(productToRemove);
            }
            return Task.CompletedTask;
        }

        public Task<List<Product>> GetAllAsync()
        {
            return Task.FromResult(_products);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await Task.FromResult(_products.FirstOrDefault(p => p.ProductID == id));
        }

        public Task<bool> IsEmpty()
        {
            return Task.FromResult(_products.Count == 0);
        }
    }
}

