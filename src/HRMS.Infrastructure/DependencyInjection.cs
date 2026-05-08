using HRMS.Application.Common.Interfaces;
using HRMS.Infrastructure.Authentication;
using HRMS.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IPayrollEngine, PayrollEngine>();
        services.AddScoped<ITenantContext, HRMS.Infrastructure.Tenancy.TenantContext>();

        // Performance & Resilience
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService(sp => (BackgroundJobService)sp.GetRequiredService<IBackgroundJobService>());

        return services;
    }
}
