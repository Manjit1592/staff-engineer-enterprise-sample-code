using Enterprise.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ProductService.Persistence.Context;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}