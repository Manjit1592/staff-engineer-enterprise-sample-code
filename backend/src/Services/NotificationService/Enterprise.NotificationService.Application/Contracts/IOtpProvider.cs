namespace Enterprise.NotificationService.Application.Contracts;

public interface IOtpProvider
{
    Task SendAsync(
        string phoneNumber,
        string otp);
}