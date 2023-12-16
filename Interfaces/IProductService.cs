using Models;

namespace Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> AddProductAsync(ProductDto product);    
    Task<Product?> DeleteProductAsync(int id);
}