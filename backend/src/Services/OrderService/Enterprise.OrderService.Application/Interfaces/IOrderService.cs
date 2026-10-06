using Enterprise.OrderService.Application.DTOs;
using Enterprise.SharedKernel.Responses;

namespace Enterprise.OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<Result> PlaceOrderAsync(PlaceOrderRequest request);
    Task<Result<List<AdminOrderViewDto>>> GetAllOrdersAsync();
Task<Result> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
}