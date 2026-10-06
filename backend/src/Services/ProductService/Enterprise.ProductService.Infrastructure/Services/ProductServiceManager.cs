using Enterprise.EventBus.Kafka.Events;
using Enterprise.EventBus.Kafka.Producers;
using Enterprise.EventBus.RabbitMQ.Events;
using Enterprise.EventBus.RabbitMQ.Publishers;
using Enterprise.ProductService.Application.DTOs;
using Enterprise.ProductService.Application.Interfaces;
using Enterprise.ProductService.Domain.Entities;
using Enterprise.ProductService.Infrastructure.Search;
using Enterprise.ProductService.Persistence.Context;
using Enterprise.SharedKernel.Responses;
using Microsoft.EntityFrameworkCore;
using Enterprise.ProductService.Infrastructure.Clients;

namespace Enterprise.ProductService.Infrastructure.Services;

public class ProductServiceManager : IProductService
{
    private readonly ProductDbContext _context;
    private readonly RabbitMqPublisher _publisher;
    private readonly KafkaProducer _kafkaProducer;
    private readonly ProductElasticIndexer _elasticIndexer;
    private readonly IInventoryClient _inventoryClient;

    public ProductServiceManager(
    ProductDbContext context,
    RabbitMqPublisher publisher,
    KafkaProducer kafkaProducer,
    ProductElasticIndexer elasticIndexer,
    IInventoryClient inventoryClient)
{
    _context = context;
    _publisher = publisher;
    _kafkaProducer = kafkaProducer;
    _elasticIndexer = elasticIndexer;
    _inventoryClient = inventoryClient;
}

    public async Task<Result> CreateProductAsync(
        CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            SKU = request.SKU
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();    

        var productCreatedEvent = new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            SKU = product.SKU,
            CreatedOnUtc = product.CreatedOnUtc
        };

        try
        {
            _publisher.Publish(
                "product-created-queue",
                productCreatedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"RabbitMQ publish failed: {ex.Message}");
        }

        var productAuditEvent = new ProductAuditEvent
        {
            ProductId = product.Id,
            Action = "PRODUCT_CREATED",
            ProductName = product.Name,
            Price = product.Price,
            OccurredOnUtc = DateTime.UtcNow
        };

        try
        {
            await _kafkaProducer.PublishAsync(
                "product-audit-topic",
                productAuditEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Kafka publish failed: {ex.Message}");
        }

        try
        {
            await _elasticIndexer.IndexProductAsync(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Elastic index failed: {ex.Message}");
        }

        return Result.Ok("Product created successfully");
    }

public async Task<Result<List<Product>>> GetAllProductsAsync()
{
    var products = await _context.Products.ToListAsync();

    var tenantId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    foreach (var product in products)
{
    var liveStock =
        await _inventoryClient.GetAvailableQuantityAsync(
            product.Id,
            tenantId);

    if (liveStock.HasValue)
    {
        product.StockQuantity = liveStock.Value;
    }
    else
    {
        Console.WriteLine(
            $"Using fallback stock for Product: {product.Name}");

        // Keep the existing Product.StockQuantity from the database.
    }
}

    return Result<List<Product>>.Ok(products);
}
    public async Task<Result<List<Product>>> SearchProductsAsync(
        string term)
    {
        var products =
            await _elasticIndexer.SearchProductsAsync(term);

        return Result<List<Product>>.Ok(products);
    }
}