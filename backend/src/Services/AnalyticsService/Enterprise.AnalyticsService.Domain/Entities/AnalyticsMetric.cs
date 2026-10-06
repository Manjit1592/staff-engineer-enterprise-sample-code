namespace Enterprise.AnalyticsService.Domain.Entities;

public class AnalyticsMetric
{
    public Guid Id { get; set; }

    public DateTime MetricDate { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal AverageOrderValue { get; set; }

    public DateTime CreatedOnUtc { get; set; }
        = DateTime.UtcNow;
}