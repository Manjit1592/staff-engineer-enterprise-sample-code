using Enterprise.NotificationService.Domain.Enums;

namespace Enterprise.NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public NotificationType Type { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; }
        = NotificationStatus.Pending;

    public int RetryCount { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? SentOnUtc { get; set; }

public DateTime? NextAttemptUtc { get; set; }

public string? FailureReason { get; set; }
}