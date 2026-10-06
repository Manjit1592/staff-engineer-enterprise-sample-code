using System.Net.Http.Json;

namespace Enterprise.ProductService.Infrastructure.Clients;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int?> GetAvailableQuantityAsync(
    Guid productId,
    Guid tenantId)
{
    try
    {
        var response =
            await _httpClient.GetFromJsonAsync<InventoryQuantityResponse>(
                $"/api/inventory/{tenantId}/{productId}/available");

        return response?.AvailableQuantity;
    }
    catch (Exception ex)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("Inventory Service Unavailable");
        Console.WriteLine(ex.Message);
        Console.WriteLine("Using fallback stock quantity");
        Console.WriteLine("=================================");

        return null;
    }
}

    private class InventoryQuantityResponse
    {
        public int AvailableQuantity { get; set; }
    }
}