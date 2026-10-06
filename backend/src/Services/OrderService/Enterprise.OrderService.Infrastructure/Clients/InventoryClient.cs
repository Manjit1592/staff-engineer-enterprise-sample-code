using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Enterprise.OrderService.Infrastructure.Clients;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;
private readonly ILogger<InventoryClient> _logger;

    public InventoryClient(
    HttpClient httpClient,
    ILogger<InventoryClient> logger)
{
    _httpClient = httpClient;
    _logger = logger;
}

public async Task<bool> ReserveStockAsync(
    Guid productId,
    Guid tenantId,
    int quantity,
    Guid orderId)
{
    try
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "/api/inventory/reserve",
                new
                {
                    ProductId = productId,
                    TenantId = tenantId,
                    Quantity = quantity,
                    OrderId = orderId
                });

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
    "Inventory reservation failed. ProductId: {ProductId}, TenantId: {TenantId}, OrderId: {OrderId}, Response: {Response}",
    productId,
    tenantId,
    orderId,
    error);

            return false;
        }

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(
    ex,
    "Inventory service unreachable. ProductId: {ProductId}, TenantId: {TenantId}, Quantity: {Quantity}, OrderId: {OrderId}",
    productId,
    tenantId,
    quantity,
    orderId);

        return false;
    }
}
}