using Enterprise.NotificationService.Application.Contracts;
using Enterprise.NotificationService.Application.Services;
using Enterprise.NotificationService.Infrastructure.Providers;
using Enterprise.NotificationService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IEmailProvider,
            SmtpEmailProvider>();

        services.AddScoped<IPushProvider,
            DummyPushProvider>();

        services.AddScoped<IOtpProvider,
            DummyOtpProvider>();

        services.AddScoped<INotificationService,
    NotificationManager>();

        return services;
    }
}