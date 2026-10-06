using Enterprise.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.AuthService.Persistence.Context;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
}