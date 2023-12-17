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
}