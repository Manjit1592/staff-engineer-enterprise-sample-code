using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Enterprise.RagService.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.RagService.Infrastructure.Security;

public class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateToken(
        string userId,
        string tenantId,
        IEnumerable<string> roles)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "UserId is required.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException(
                "TenantId is required.",
                nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(
                _options.SigningKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured.");
        }

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    userId),

                new(
                    ClaimTypes.NameIdentifier,
                    userId),

                new(
                    "tenant_id",
                    tenantId)
            };

        foreach (var role in roles.Distinct(
                     StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.SigningKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(
                        _options.ExpirationMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}