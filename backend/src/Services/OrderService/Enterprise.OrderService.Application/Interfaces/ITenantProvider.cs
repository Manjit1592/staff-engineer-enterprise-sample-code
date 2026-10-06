namespace Enterprise.OrderService.Application.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
}