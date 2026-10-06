using Enterprise.Logging.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureEnterpriseLogging("ApiGateway");

builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,
    reloadOnChange: true);

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

builder.Services.AddOcelot();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                var key =
                    context.User.Identity?.IsAuthenticated == true
                        ? context.User.Identity?.Name ??
                          context.Connection.RemoteIpAddress?.ToString() ??
                          "authenticated"
                        : context.Connection.RemoteIpAddress?.ToString() ??
                          "anonymous";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey: key,
                        factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 100,
                                Window =
                                    TimeSpan.FromMinutes(1),

                                QueueProcessingOrder =
                                    QueueProcessingOrder
                                        .OldestFirst,

                                QueueLimit = 10
                            });
            });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

await app.UseOcelot();

app.Run();