using Enterprise.NotificationService.Application.Contracts;

namespace Enterprise.NotificationService.Infrastructure.Providers;

public class DummyOtpProvider : IOtpProvider
{
    public Task SendAsync(
        string phoneNumber,
        string otp)
    {
        Console.WriteLine(
            $"OTP => Phone:{phoneNumber}");

        return Task.CompletedTask;
    }
}