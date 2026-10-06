using Enterprise.AnalyticsService.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.AnalyticsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsDbContext _dbContext;

    public AnalyticsController(
        AnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var metrics =
            await _dbContext.AnalyticsMetrics
                .OrderByDescending(x => x.MetricDate)
                .FirstOrDefaultAsync();

        return Ok(metrics);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue()
    {
        var revenue =
            await _dbContext.AnalyticsMetrics
                .SumAsync(x => x.TotalRevenue);

        return Ok(new
        {
            TotalRevenue = revenue
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders =
            await _dbContext.AnalyticsMetrics
                .SumAsync(x => x.TotalOrders);

        return Ok(new
        {
            TotalOrders = orders
        });
    }
}