using System.Collections.Concurrent;
using HRMS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HRMS.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly ConcurrentDictionary<string, byte> CacheKeys = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        return Task.FromResult(cache.Get<T>(key));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = expiration ?? TimeSpan.FromMinutes(30)
        };

        cache.Set(key, value, options);
        CacheKeys.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        cache.Remove(key);
        CacheKeys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keysToRemove = CacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            CacheKeys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
