using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Persistence.Context;
using HRMS.Persistence.Interceptors;
using HRMS.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, AuditLogInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        services.AddScoped<IExpenseClaimRepository, ExpenseClaimRepository>();

        services.AddScoped<ITravelRequestRepository, TravelRequestRepository>();

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
