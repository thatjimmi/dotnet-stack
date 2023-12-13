using Interfaces;
using Models;
using System.Collections.Concurrent;

namespace Data;

public class InMemoryDatabaseContext : IDatabaseContext
{
    // Thread-safe collection to mimic database table
    private readonly ConcurrentDictionary<int, Product> _products = new();

    public Task<Product?> GetProductByIdAsync(int id)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task AddProductAsync(Product product)
    {
        // Calculate the next ID
        var nextId = _products.IsEmpty ? 1 : _products.Keys.Max() + 1;

        product.ProductID = nextId;

        _products.TryAdd(nextId, product);
        return Task.CompletedTask;
    }

    public Task<List<Product>> GetProductsAsync()
    {
        var products = _products.Values.ToList();
        return Task.FromResult(products);
    }

    public Task DeleteProductAsync(int id)
    {
        _products.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
