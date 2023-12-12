using Models;

namespace Interfaces;

public interface IDatabaseContext
{
    Task<Product?> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
    Task<List<Product>> GetProductsAsync();
    Task DeleteProductAsync(int id);
}