using Microsoft.EntityFrameworkCore;
using Models;

namespace Interfaces;

public interface IDatabaseContext
{
    DbSet<Product> Products { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}