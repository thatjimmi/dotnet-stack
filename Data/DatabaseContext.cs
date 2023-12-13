using Microsoft.EntityFrameworkCore;
using Interfaces;
using Models;

namespace Data;

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) {}

    protected DbSet<Product> Products { get; set; }

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
}