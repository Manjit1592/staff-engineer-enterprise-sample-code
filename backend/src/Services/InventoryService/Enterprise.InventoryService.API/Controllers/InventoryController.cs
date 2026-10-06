using Enterprise.InventoryService.Application.DTOs;
using Enterprise.InventoryService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.InventoryService.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(
        IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(
        ReserveStockRequest request)
    {
        var result =
            await _inventoryService.ReserveStockAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

[HttpGet("{tenantId:guid}/{productId:guid}/available")]
public async Task<IActionResult> GetAvailableQuantity(
    Guid tenantId,
    Guid productId)
{
    var quantity =
        await _inventoryService.GetAvailableQuantityAsync(
            tenantId,
            productId);

    return Ok(new
    {
        AvailableQuantity = quantity
    });
}
}