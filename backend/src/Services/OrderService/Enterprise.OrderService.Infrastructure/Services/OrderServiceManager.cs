using Enterprise.AuthService.Persistence.Context;
using Enterprise.EventBus.Kafka.Events;
using Enterprise.EventBus.Kafka.Producers;
using Enterprise.Common.Events;
using Enterprise.EventBus.RabbitMQ.Publishers;
using Enterprise.OrderService.Application.DTOs;
using Enterprise.OrderService.Application.Interfaces;
using Enterprise.OrderService.Domain.Entities;
using Enterprise.OrderService.Persistence.Context;
using Enterprise.ProductService.Persistence.Context;
using Enterprise.SharedKernel.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Enterprise.OrderService.Infrastructure.BackgroundJobs;
using Enterprise.OrderService.Infrastructure.Clients;

namespace Enterprise.OrderService.Infrastructure.Services;

public class OrderServiceManager : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly ProductDbContext _productContext;
    private readonly AuthDbContext _authContext;
    private readonly RabbitMqPublisher _publisher;
    private readonly KafkaProducer _kafkaProducer;
    private readonly IDistributedCache _cache;
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly ITenantProvider _tenantProvider;	
    private readonly IInventoryClient _inventoryClient;

    public OrderServiceManager(
        OrderDbContext context,
        ProductDbContext productContext,
        AuthDbContext authContext,
        RabbitMqPublisher publisher,
        KafkaProducer kafkaProducer,
        IDistributedCache cache,
        BackgroundTaskQueue taskQueue,
	ITenantProvider tenantProvider,
	IInventoryClient inventoryClient)
    {
        _context = context;
        _productContext = productContext;
        _authContext = authContext;
        _publisher = publisher;
        _kafkaProducer = kafkaProducer;
        _cache = cache;
        _taskQueue = taskQueue;
        _tenantProvider = tenantProvider;
	_inventoryClient = inventoryClient;
    }

    public async Task<Result> PlaceOrderAsync(
        PlaceOrderRequest request)
    {

Console.WriteLine("===== TENANT DEBUG =====");
Console.WriteLine($"TenantProvider: {_tenantProvider.TenantId}");

        var order = new Order
{
    UserId = request.UserId,
    TenantId = _tenantProvider.TenantId,
    Status = "PLACED"
};

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _productContext.Products
                .FirstOrDefaultAsync(
                    x => x.Id == item.ProductId);

            if (product == null)
            {
                return Result.Fail(
                    $"Product not found: {item.ProductId}");
            }

            var reserved =
    await _inventoryClient.ReserveStockAsync(
        product.Id,
        _tenantProvider.TenantId,
        item.Quantity,
        order.Id);

if (!reserved)
{
    return Result.Fail(
    $"Inventory service is temporarily unavailable. Unable to place the order for '{product.Name}'. Please try again in a few moments.");
}

            var lineTotal =
                product.Price * item.Quantity;

            totalAmount += lineTotal;

            

            order.Items.Add(
                new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });
        }

        order.TotalAmount = totalAmount;
Console.WriteLine($"Order TenantId: {order.TenantId}");
Console.WriteLine("STEP 1 - Before creating outbox");
var outboxMessage = new OutboxMessage
{
    EventType = nameof(OrderCreatedEvent),
    Payload = JsonSerializer.Serialize(
        new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = totalAmount,
            Status = order.Status,
            CreatedOnUtc = order.CreatedOnUtc
        }),
    IsProcessed = false
};

_context.OutboxMessages.Add(outboxMessage);

        _context.Orders.Add(order);

        Console.WriteLine("STEP 2 - Before SaveChanges");
        await _context.SaveChangesAsync();
Console.WriteLine("STEP 3 - After SaveChanges");
        var user = await _authContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == order.UserId);

        var orderCreatedEvent =
            new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                CustomerEmail =
                    user?.Email ?? string.Empty,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedOnUtc = order.CreatedOnUtc
            };

    //    try
      //  {
        //    _publisher.Publish(
          //      "order-created-queue",
            //    orderCreatedEvent);
        //}
      //  catch (Exception ex)
        //{
          //  Console.WriteLine(
            //    $"RabbitMQ order publish failed: {ex.Message}");
        //}

        var orderAuditEvent =
            new OrderAuditEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Action = "ORDER_PLACED",
                TotalAmount = order.TotalAmount,
                OccurredOnUtc = DateTime.UtcNow
            };

    //    await _taskQueue.QueueBackgroundWorkItemAsync(
      //      async token =>
        //    {
          //      try
            //    {
              //      Console.WriteLine(
                //        "PROCESSING BACKGROUND KAFKA EVENT");
//
  //                  await _kafkaProducer.PublishAsync(
    //                    "order-audit-topic",
      //                  orderAuditEvent);
//
  //                  Console.WriteLine(
    //                    "BACKGROUND KAFKA EVENT COMPLETED");
      //          }
        //        catch (Exception ex)
          //      {
            //        Console.WriteLine(
              //          $"Background Kafka publish failed: {ex.Message}");
                //}
            //});

        await _cache.RemoveAsync("all_orders");
Console.WriteLine("STEP 4 - Returning success");
        return Result.Ok(
            "Order placed successfully");
    }

    public async Task<Result<List<AdminOrderViewDto>>>
        GetAllOrdersAsync()
    {
        const string cacheKey = "all_orders";

        var cachedOrders =
            await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedOrders))
        {
            Console.WriteLine(
                "ORDERS LOADED FROM REDIS CACHE");

            var cachedResult =
                JsonSerializer.Deserialize<
                    List<AdminOrderViewDto>>(
                    cachedOrders);

            return Result<List<AdminOrderViewDto>>
                .Ok(
                    cachedResult ??
                    new List<AdminOrderViewDto>());
        }

        Console.WriteLine(
            "ORDERS LOADED FROM SQL");

        var orders = await _context.Orders
            .OrderByDescending(
                x => x.CreatedOnUtc)
            .ToListAsync();

        var userIds = orders
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        var users = await _authContext.Users
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync();

        var result = orders.Select(
            (order, index) =>
            {
                var user =
                    users.FirstOrDefault(
                        x => x.Id == order.UserId);

                return new AdminOrderViewDto
                {
                    Id = order.Id,

                    OrderNumber =
                        $"ORD-{1000 + index + 1}",

                    CustomerName =
                        user != null
                            ? $"{user.FirstName} {user.LastName}"
                            : "Unknown User",

                    CustomerEmail =
                        user?.Email ?? "Unknown",

                    TotalAmount =
                        order.TotalAmount,

                    Status =
                        order.Status,

                    CreatedOnUtc =
                        order.CreatedOnUtc
                };
            })
            .ToList();

        var cacheOptions =
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromSeconds(30)
            };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            cacheOptions);

        return Result<List<AdminOrderViewDto>>
            .Ok(result);
    }

    public async Task<Result> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request)
    {
        var allowedStatuses =
            new[]
            {
                "PLACED",
                "PROCESSING",
                "SHIPPED",
                "DELIVERED",
                "CANCELLED"
            };

        if (!allowedStatuses.Contains(
                request.Status.ToUpper()))
        {
            return Result.Fail(
                "Invalid order status");
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == orderId);

        if (order == null)
        {
            return Result.Fail(
                "Order not found");
        }

        order.Status =
            request.Status.ToUpper();

        await _context.SaveChangesAsync();

        await _cache.RemoveAsync(
            "all_orders");

        return Result.Ok(
            "Order status updated");
    }
}