using Enterprise.OrderService.Application.Interfaces;
using Enterprise.OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.OrderService.Persistence.Context;

public class OrderDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public OrderDbContext(
        DbContextOptions<OrderDbContext> options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId);

        modelBuilder.Entity<Order>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(x => x.TenantId);

        modelBuilder.Entity<Order>()
            .HasIndex(x => x.TenantId);

        modelBuilder.Entity<Order>()
            .HasQueryFilter(
                x => x.TenantId ==
                     _tenantProvider.TenantId);

        modelBuilder.Entity<OrderItem>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);
       
  	modelBuilder.Entity<OutboxMessage>()
    .Property(x => x.EventType)
    .HasMaxLength(200);

modelBuilder.Entity<OutboxMessage>()
    .Property(x => x.Payload)
    .HasColumnType("nvarchar(max)");

modelBuilder.Entity<OutboxMessage>()
    .HasIndex(x => x.IsProcessed);
    }
}