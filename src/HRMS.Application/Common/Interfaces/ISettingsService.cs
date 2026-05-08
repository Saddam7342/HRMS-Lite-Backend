namespace HRMS.Application.Common.Interfaces;

public interface ISettingsService
{
    Task<T?> GetSettingAsync<T>(string key, CancellationToken ct = default);
    Task<string> GetSettingValueAsync(string key, string defaultValue = "", CancellationToken ct = default);
    Task<bool> IsFeatureEnabledAsync(string featureKey, CancellationToken ct = default);
    Task ClearCacheAsync(Guid tenantId);
}
