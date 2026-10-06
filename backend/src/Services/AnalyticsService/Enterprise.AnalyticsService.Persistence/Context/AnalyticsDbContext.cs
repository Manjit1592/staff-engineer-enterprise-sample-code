using Enterprise.AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.AnalyticsService.Persistence.Context;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(
        DbContextOptions<AnalyticsDbContext> options)
        : base(options)
    {
    }

    public DbSet<AnalyticsMetric> AnalyticsMetrics =>
        Set<AnalyticsMetric>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}