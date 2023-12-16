using Microsoft.EntityFrameworkCore;
using Interfaces;
using Models;

namespace Data;

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) {}

    public DbSet<Product> Products { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
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