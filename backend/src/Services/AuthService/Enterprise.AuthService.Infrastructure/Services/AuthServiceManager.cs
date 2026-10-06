using Enterprise.AuthService.Application.DTOs;
using Enterprise.AuthService.Application.Interfaces;
using Enterprise.AuthService.Domain.Entities;
using Enterprise.AuthService.Infrastructure.Security;
using Enterprise.AuthService.Persistence.Context;
using Enterprise.SharedKernel.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.AuthService.Infrastructure.Services;

public class AuthServiceManager : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthServiceManager(AuthDbContext context, JwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var exists = await _context.Users.AnyAsync(x => x.Email == request.Email);

        if (exists)
            return Result.Fail("User already exists");

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Result.Ok("User registered successfully");
    }

    public async Task<Result<string>> LoginAsync(LoginRequest request)
    {
        var hashedPassword = HashPassword(request.Password);

        var user = await _context.Users.FirstOrDefaultAsync(x =>
            x.Email == request.Email && x.PasswordHash == hashedPassword);

        if (user == null)
            return Result<string>.Fail("Invalid credentials");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result<string>.Ok(token);
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}