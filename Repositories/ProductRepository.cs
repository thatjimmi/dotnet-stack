using Microsoft.EntityFrameworkCore;
using Interfaces;
using Models;

namespace Repositories;

public class ProductRepository : IRepository<Product>
{
    private readonly IDatabaseContext _context;    

    public ProductRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();        
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException("Product not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var products = await _context.Products.ToListAsync();
        
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        return product;
    }

    public async Task<bool> IsEmpty()
    {
        return await _context.Products.CountAsync() == 0;
    }   
}
