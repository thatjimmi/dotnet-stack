using Interfaces;
using Models;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;         
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {            
            var products = await _repository.GetAllAsync();         
            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {            
            var product = await _repository.GetByIdAsync(id);            
            return product;
        }

        public async Task<Product> AddProductAsync(ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Quantity = productDto.Quantity
            };

            await _repository.AddAsync(product);           

            return product;
        }

        public async Task<Product?> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product != null)
            {
                await _repository.DeleteAsync(product.ProductID);                
            }

            return product;
        }
    }
}