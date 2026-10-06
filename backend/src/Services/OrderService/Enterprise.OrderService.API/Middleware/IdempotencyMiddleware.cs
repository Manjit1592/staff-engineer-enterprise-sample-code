using Enterprise.OrderService.API.Services;
using Microsoft.AspNetCore.Http;

namespace Enterprise.OrderService.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IdempotencyService _idempotencyService;

    public IdempotencyMiddleware(
        RequestDelegate next,
        IdempotencyService idempotencyService)
    {
        _next = next;
        _idempotencyService = idempotencyService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/orders/place"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(
                "Missing Idempotency-Key header");

            return;
        }

        if (!await _idempotencyService.TryAcquireAsync(key!))
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsync("Duplicate request");

            return;
        }

        try
        {
            await _next(context);

            if (context.Response.StatusCode < 400)
            {
                await _idempotencyService.MarkCompletedAsync(key!);
            }
            else
            {
                await _idempotencyService.RemoveAsync(key!);
            }
        }
        catch
        {
            await _idempotencyService.RemoveAsync(key!);
            throw;
        }
    }
}