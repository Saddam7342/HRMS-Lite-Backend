using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HRMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<AppUser> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    
    DbSet<Employee> Employees { get; }
    DbSet<Department> Departments { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<ExpenseClaim> ExpenseClaims { get; }
    DbSet<TravelRequest> TravelRequests { get; }
    DbSet<Notification> Notifications { get; }

    DbSet<SalaryStructure> SalaryStructures { get; }
    DbSet<Payroll> Payrolls { get; }

    Guid TenantId { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
