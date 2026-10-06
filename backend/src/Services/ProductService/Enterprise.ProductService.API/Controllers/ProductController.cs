using Enterprise.ProductService.Application.DTOs;
using Enterprise.ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Enterprise.ProductService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "Admin")]
[HttpPost("create")]
public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var result = await _productService.CreateProductAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,User")]
[HttpGet("all")]
public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsAsync();
        return Ok(result);
    }

    [Authorize(Roles = "Admin,User")]
[HttpGet("search")]
public async Task<IActionResult> Search([FromQuery] string term)
    {
        var result = await _productService.SearchProductsAsync(term);
        return Ok(result);
    }
}