using Enterprise.NotificationService.Application.Contracts;
using Enterprise.NotificationService.Application.DTOs;
using Enterprise.NotificationService.Application.Services;
using Enterprise.NotificationService.Domain.Entities;
using Enterprise.NotificationService.Domain.Enums;
using Enterprise.NotificationService.Persistence.Context;

namespace Enterprise.NotificationService.Infrastructure.Services;

public class NotificationManager : INotificationService
{
    private readonly IEmailProvider _emailProvider;
    private readonly IPushProvider _pushProvider;
    private readonly IOtpProvider _otpProvider;
    private readonly NotificationDbContext _dbContext;

    public NotificationManager(
        IEmailProvider emailProvider,
        IPushProvider pushProvider,
        IOtpProvider otpProvider,
        NotificationDbContext dbContext)
    {
        _emailProvider = emailProvider;
        _pushProvider = pushProvider;
        _otpProvider = otpProvider;
        _dbContext = dbContext;
    }

    public async Task SendAsync(NotificationRequest request)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Recipient = request.Recipient,
            Subject = request.Subject,
            Message = request.Message,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedOnUtc = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync();

        return;
    }
}