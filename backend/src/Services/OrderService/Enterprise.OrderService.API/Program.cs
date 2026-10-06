using Enterprise.OrderService.Application.Interfaces;
using Enterprise.OrderService.API;
using Enterprise.OrderService.Infrastructure.Services;
using Enterprise.OrderService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Enterprise.ProductService.Persistence.Context;
using Enterprise.AuthService.Persistence.Context;
using Enterprise.EventBus.Kafka.Producers;
using Enterprise.EventBus.RabbitMQ.Publishers;
using Enterprise.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Enterprise.OrderService.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Enterprise.Logging.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Confluent.Kafka;
using Enterprise.OrderService.Infrastructure.Clients;
using Microsoft.Extensions.Http.Resilience;
using Enterprise.OrderService.API.Middleware;
using Enterprise.OrderService.API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureEnterpriseLogging("OrderService");

builder.Services.AddControllers();



builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress =
        new Uri("http://inventoryservice:8080");
})
.AddStandardResilienceHandler(options =>
{
    // Retry Policy
options.Retry.MaxRetryAttempts = 3;
options.Retry.Delay = TimeSpan.FromSeconds(1);
options.Retry.UseJitter = true;

options.Retry.OnRetry = static args =>
{
    Console.WriteLine(
        $"[RETRY] Attempt={args.AttemptNumber + 1}, Delay={args.RetryDelay}, Exception={args.Outcome.Exception?.Message}");

    return ValueTask.CompletedTask;
};

    // Each individual HTTP attempt
    options.AttemptTimeout.Timeout =
        TimeSpan.FromSeconds(3);

    // Entire operation (including retries)
    options.TotalRequestTimeout.Timeout =
        TimeSpan.FromSeconds(10);

// Circuit Breaker
options.CircuitBreaker.FailureRatio = 0.5;
options.CircuitBreaker.MinimumThroughput = 4;
options.CircuitBreaker.SamplingDuration =
    TimeSpan.FromSeconds(10);
options.CircuitBreaker.BreakDuration =
    TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "host.docker.internal:6379";
    options.InstanceName = "EnterpriseOrderPlatform";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect("host.docker.internal:6379"));

builder.Services.AddSingleton<IdempotencyService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AuthDb")));

builder.Services.AddScoped<RabbitMqPublisher>();
builder.Services.AddSingleton(
    new ProducerConfig
    {
        BootstrapServers =
            builder.Configuration["Kafka:BootstrapServers"]!
    });

builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddScoped<IOrderService, OrderServiceManager>();

builder.Services.AddSingleton<BackgroundTaskQueue>();

builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHostedService<OutboxPublisher>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("OrderService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

var jwtSecret =
    builder.Configuration["JwtSettings:Secret"]!;

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RoleClaimType =
                    System.Security.Claims.ClaimTypes.Role,
                ValidIssuer =
                    builder.Configuration["JwtSettings:Issuer"],
                ValidAudience =
                    builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret))
            };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
    "http://localhost:5173",
    "http://localhost:5500")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services
    .AddHealthChecks()
    .AddSqlServer(
        builder.Configuration["ConnectionStrings:OrderDb"]!,
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers();

app.MapHub<NotificationHub>(
    "/realtime/orders");

app.MapHealthChecks("/health");

app.Run();