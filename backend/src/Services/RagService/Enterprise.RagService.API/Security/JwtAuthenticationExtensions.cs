using System.Security.Claims;
using System.Text;
using Enterprise.RagService.Application.Identity;
using Enterprise.RagService.Application.Security;
using Enterprise.RagService.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.RagService.API.Security;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddEnterpriseJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("Jwt"));

        var jwtOptions =
            configuration
                .GetSection("Jwt")
                .Get<JwtOptions>()
            ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(
                jwtOptions.SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured.");
        }

        services.AddHttpContextAccessor();

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer =
                                jwtOptions.Issuer,

                            ValidateAudience = true,
                            ValidAudience =
                                jwtOptions.Audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SigningKey)),

                            ValidateLifetime = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            RoleClaimType =
                                ClaimTypes.Role,

                            NameClaimType =
                                ClaimTypes.NameIdentifier
                        };
                });

        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(
                    SecurityPolicies.McpUser,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });

                options.AddPolicy(
                    SecurityPolicies.McpAdmin,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireRole(
                            SecurityRoles.Admin);
                    });

                options.AddPolicy(
                    SecurityPolicies.RagUser,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });
            });

        services.AddScoped<
            IUserIdentity>(
            provider =>
            {
                var httpContextAccessor =
                    provider.GetRequiredService<
                        IHttpContextAccessor>();

                var principal =
                    httpContextAccessor
                        .HttpContext?
                        .User
                    ?? new ClaimsPrincipal();

                return new DevelopmentUserIdentity(
                    principal);
            });

        /*
         * MCP context is intentionally a separate abstraction
         * from IUserIdentity because the existing MCP protocol
         * boundary requires a single Role value.
         *
         * Production can map this from authenticated claims
         * without trusting request-body identity information.
         */
        services.AddScoped<
            IMcpUserContext>(
            provider =>
            {
                var httpContextAccessor =
                    provider.GetRequiredService<
                        IHttpContextAccessor>();

                var principal =
                    httpContextAccessor
                        .HttpContext?
                        .User
                    ?? new ClaimsPrincipal();

                var tenantId =
                    principal.FindFirst(
                        "tenant_id")?.Value
                    ?? string.Empty;

                var userId =
                    principal.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value
                    ?? principal.FindFirst(
                        "sub")?.Value
                    ?? string.Empty;

                var role =
                    principal.FindFirst(
                        ClaimTypes.Role)?.Value
                    ?? string.Empty;

                return new McpUserContext
                {
                    TenantId = tenantId,
                    UserId = userId,
                    Role = role,
                    IsAuthenticated =
                        principal.Identity?
                            .IsAuthenticated == true
                };
            });

        services.AddScoped<
            IAuthorizationService,
            DevelopmentAuthorizationService>();

        services.AddScoped<
            JwtTokenService>();

        return services;
    }
}