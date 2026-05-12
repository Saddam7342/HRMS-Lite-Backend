using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HRMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }

    DbSet<AppUser> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Employee> Employees { get; }
    DbSet<Department> Departments { get; }

    DbSet<LeaveType> LeaveTypes { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }

    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<ExpenseClaim> ExpenseClaims { get; }

    DbSet<TravelRequest> TravelRequests { get; }

    DbSet<AttendanceRecord> AttendanceRecords { get; }

    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreferences> NotificationPreferences { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Document> Documents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
