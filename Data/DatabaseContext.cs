using Microsoft.EntityFrameworkCore;
using Interfaces;
using Models;

namespace Data;

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) {}

    public DbSet<Product> Products { get; set; }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await Products.FindAsync(id);
    }

    public async Task AddProductAsync(Product product)
    {
        await Products.AddAsync(product);
        await SaveChangesAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await Products.ToListAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await Products.FindAsync(id) ?? throw new KeyNotFoundException("Product not found");
        Products.Remove(product);
        await SaveChangesAsync();
    }

    public static List<Product> GenerateSeedData(int numberOfProducts)
    {
        var products = new List<Product>();
        for (var i = 0; i < numberOfProducts; i++) {
            var product = new Product
            {
                Name = Faker.Internet.DomainWord(),
                Price = Faker.RandomNumber.Next(0, 10000),
                Quantity = Faker.RandomNumber.Next(0, 100)
            };

            products.Add(product);
        }

        return products;
    }    
}