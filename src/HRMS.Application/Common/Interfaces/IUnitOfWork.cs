namespace HRMS.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IApplicationDbContext DbContext { get; }
    IOrganizationRepository Organizations { get; }
    IUserRepository Users { get; }
    IEmployeeRepository Employees { get; }
    IDepartmentRepository Departments { get; }
    
    ILeaveTypeRepository LeaveTypes { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    ILeaveRequestRepository LeaveRequests { get; }

    IExpenseCategoryRepository ExpenseCategories { get; }
    IExpenseClaimRepository ExpenseClaims { get; }

    ITravelRequestRepository TravelRequests { get; }

    IAttendanceRepository Attendance { get; }

    INotificationRepository Notifications { get; }
    INotificationPreferencesRepository NotificationPreferences { get; }

    IAuditLogRepository AuditLogs { get; }

    IOrganizationSettingRepository Settings { get; }

    IDocumentRepository Documents { get; }

    IPayrollRepository Payroll { get; }
    
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
