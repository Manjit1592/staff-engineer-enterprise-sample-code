using Enterprise.AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Enterprise.AuthService.Infrastructure.Security;

public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(
        ApplicationUser user)
    {
        var secret =
            _configuration["JwtSettings:Secret"]!;

        var issuer =
            _configuration["JwtSettings:Issuer"]!;

        var audience =
            _configuration["JwtSettings:Audience"]!;

        var expiryMinutes =
            Convert.ToDouble(
                _configuration[
                    "JwtSettings:ExpiryMinutes"]);

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                "TenantId",
                user.TenantId.ToString()),

            new Claim(
                "firstName",
                user.FirstName),

            new Claim(
                "lastName",
                user.LastName)
        };

        var token =
            new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires:
                    DateTime.UtcNow.AddMinutes(
                        expiryMinutes),
                signingCredentials:
                    credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}