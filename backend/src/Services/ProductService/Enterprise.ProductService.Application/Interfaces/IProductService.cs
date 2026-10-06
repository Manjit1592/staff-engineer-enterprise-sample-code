using Enterprise.ProductService.Application.DTOs;
using Enterprise.ProductService.Domain.Entities;
using Enterprise.SharedKernel.Responses;

namespace Enterprise.ProductService.Application.Interfaces;

public interface IProductService
{
    Task<Result> CreateProductAsync(CreateProductRequest request);
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<List<Product>>> SearchProductsAsync(string term);
}