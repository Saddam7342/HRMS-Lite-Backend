using System.Collections.Concurrent;
using HRMS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HRMS.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache cache, ITenantContext tenantContext) : ICacheService
{
    private static readonly ConcurrentDictionary<string, byte> CacheKeys = new();

    private string GetFullKey(string key) => $"tenant_{tenantContext.TenantId}_{key}";

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var fullKey = GetFullKey(key);
        return Task.FromResult(cache.Get<T>(fullKey));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var fullKey = GetFullKey(key);
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = expiration ?? TimeSpan.FromMinutes(30)
        };

        cache.Set(fullKey, value, options);
        CacheKeys.TryAdd(fullKey, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var fullKey = GetFullKey(key);
        cache.Remove(fullKey);
        CacheKeys.TryRemove(fullKey, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = GetFullKey(prefix);
        var keysToRemove = CacheKeys.Keys.Where(k => k.StartsWith(fullPrefix)).ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            CacheKeys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
