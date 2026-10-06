using Enterprise.SharedKernel.Entities;

namespace Enterprise.AuthService.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Admin";

    public bool IsActive { get; set; } = true;
}