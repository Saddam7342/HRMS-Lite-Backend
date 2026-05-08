using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace HRMS.Infrastructure.Services;

public class SettingsService(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IMemoryCache cache) : ISettingsService
{
    private string GetCacheKey(string key) => $"setting_{tenantContext.TenantId}_{key}";

    public async Task<T?> GetSettingAsync<T>(string key, CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(key);
        if (cache.TryGetValue(cacheKey, out T? cachedValue)) return cachedValue;

        var setting = await unitOfWork.Settings.GetByKeyAsync(tenantContext.TenantId, key, ct);
        if (setting == null) return default;

        try
        {
            var value = JsonSerializer.Deserialize<T>(setting.Value);
            cache.Set(cacheKey, value, TimeSpan.FromHours(1));
            return value;
        }
        catch
        {
            return default;
        }
    }

    public async Task<string> GetSettingValueAsync(string key, string defaultValue = "", CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(key);
        if (cache.TryGetValue(cacheKey, out string? cachedValue)) return cachedValue!;

        var setting = await unitOfWork.Settings.GetByKeyAsync(tenantContext.TenantId, key, ct);
        if (setting == null) return defaultValue;

        cache.Set(cacheKey, setting.Value, TimeSpan.FromHours(1));
        return setting.Value;
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureKey, CancellationToken ct = default)
    {
        var value = await GetSettingValueAsync(featureKey, "false", ct);
        return value.ToLower() == "true";
    }

    public Task ClearCacheAsync(Guid tenantId)
    {
        // Simple memory cache doesn't support wildcard removal easily.
        // In a production app with Redis, we'd clear by pattern.
        // For now, we rely on expiration or explicit key removal if we tracked them.
        return Task.CompletedTask;
    }
}
