using Enterprise.NotificationService.Application.DTOs;
using Enterprise.NotificationService.Application.Services;
using Enterprise.NotificationService.Domain.Enums;
using Enterprise.NotificationService.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly NotificationDbContext _dbContext;

    public NotificationsController(
        INotificationService notificationService,
        NotificationDbContext dbContext)
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Send(
        NotificationRequest request)
    {
        await _notificationService.SendAsync(request);

        return Ok(
            "Notification queued successfully");
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var notifications =
            await _dbContext.Notifications
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(50)
                .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("failed")]
    public async Task<IActionResult> GetFailed()
    {
        var notifications =
            await _dbContext.Notifications
                .Where(x =>
                    x.Status ==
                    NotificationStatus.Failed)
                .OrderByDescending(x => x.CreatedOnUtc)
                .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("deadletters")]
    public async Task<IActionResult> GetDeadLetters()
    {
        var notifications =
            await _dbContext.Notifications
                .Where(x =>
                    x.Status ==
                    NotificationStatus.DeadLettered)
                .OrderByDescending(x => x.CreatedOnUtc)
                .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("retry/{id:guid}")]
    public async Task<IActionResult> Retry(
        Guid id)
    {
        var notification =
            await _dbContext.Notifications
                .FirstOrDefaultAsync(x => x.Id == id);

        if (notification is null)
        {
            return NotFound();
        }

        notification.Status =
            NotificationStatus.Pending;

        notification.NextAttemptUtc = null;

        notification.FailureReason = null;

        notification.RetryCount = 0;

        await _dbContext.SaveChangesAsync();

        return Ok(
            $"Notification {id} queued for retry");
    }
}