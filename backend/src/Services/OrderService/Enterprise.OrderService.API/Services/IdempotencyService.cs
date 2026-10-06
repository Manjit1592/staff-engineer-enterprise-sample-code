using StackExchange.Redis;

namespace Enterprise.OrderService.API.Services;

public class IdempotencyService
{
    private readonly IDatabase _database;

    public IdempotencyService(IConnectionMultiplexer multiplexer)
    {
        _database = multiplexer.GetDatabase();
    }

    public async Task<bool> TryAcquireAsync(string key)
    {
        return await _database.StringSetAsync(
            key,
            "InProgress",
            TimeSpan.FromMinutes(30),
            When.NotExists);
    }

    public async Task MarkCompletedAsync(string key)
    {
        await _database.StringSetAsync(
            key,
            "Completed",
            TimeSpan.FromHours(24));
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<string?> GetStatusAsync(string key)
    {
        var value = await _database.StringGetAsync(key);

        return value.HasValue
            ? value.ToString()
            : null;
    }
}