using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Common.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Context;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ITenantContext tenantContext) : DbContext(options), IApplicationDbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<ExpenseClaim> ExpenseClaims => Set<ExpenseClaim>();
    
    public DbSet<TravelRequest> TravelRequests => Set<TravelRequest>();
    
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<OrganizationSetting> OrganizationSettings => Set<OrganizationSetting>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();

    public Guid TenantId => tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Multi-tenancy global query filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var body = System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.TenantId)),
                    System.Linq.Expressions.Expression.Property(System.Linq.Expressions.Expression.Constant(tenantContext), nameof(ITenantContext.TenantId))
                );
                var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
