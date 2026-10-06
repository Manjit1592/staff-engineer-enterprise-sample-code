using Enterprise.EventBus.Kafka.Producers;
using Enterprise.EventBus.RabbitMQ.Consumers;
using Enterprise.EventBus.RabbitMQ.Publishers;
using Enterprise.ProductService.Application.Interfaces;
using Enterprise.ProductService.Infrastructure.Services;
using Enterprise.ProductService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Enterprise.ProductService.Infrastructure.Search;
using Enterprise.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Enterprise.Logging.Middleware;
using Enterprise.ProductService.Infrastructure.Clients;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureEnterpriseLogging("ProductService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress =
        new Uri("http://inventoryservice:8080");
});



builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddScoped<RabbitMqPublisher>();
builder.Services.AddSingleton(
    new ProducerConfig
    {
        BootstrapServers =
            builder.Configuration["Kafka:BootstrapServers"]!
    });

builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddScoped<ProductElasticIndexer>();
builder.Services.AddScoped<IProductService, ProductServiceManager>();

builder.Services.AddSingleton<ProductCreatedConsumer>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;

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
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                ValidIssuer =
                    builder.Configuration["JwtSettings:Issuer"],
                ValidAudience =
                    builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret))
            };
    });

builder.Services
    .AddHealthChecks()
    .AddSqlServer(
        builder.Configuration["ConnectionStrings:ProductDb"]!,
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

var consumer =
    app.Services.GetRequiredService<ProductCreatedConsumer>();

consumer.StartConsuming();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();