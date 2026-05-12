using HRMS.Application.Common.Interfaces;
using HRMS.Persistence.Context;

namespace HRMS.Persistence.Repositories;

public class UnitOfWork(
    AppDbContext context,
    IUserRepository users,
    IEmployeeRepository employees,
    IDepartmentRepository departments,
    ILeaveTypeRepository leaveTypes,
    ILeaveBalanceRepository leaveBalances,
    ILeaveRequestRepository leaveRequests,
    IExpenseCategoryRepository expenseCategories,
    IExpenseClaimRepository expenseClaims,
    ITravelRequestRepository travelRequests,
    IAttendanceRepository attendance,
    INotificationRepository notifications,
    INotificationPreferencesRepository notificationPreferences,
    IAuditLogRepository auditLogs,
    IDocumentRepository documents) : IUnitOfWork
{
    public IApplicationDbContext DbContext => context;
    public IUserRepository Users => users;
    public IEmployeeRepository Employees => employees;
    public IDepartmentRepository Departments => departments;

    public ILeaveTypeRepository LeaveTypes => leaveTypes;
    public ILeaveBalanceRepository LeaveBalances => leaveBalances;
    public ILeaveRequestRepository LeaveRequests => leaveRequests;

    public IExpenseCategoryRepository ExpenseCategories => expenseCategories;
    public IExpenseClaimRepository ExpenseClaims => expenseClaims;

    public ITravelRequestRepository TravelRequests => travelRequests;

    public IAttendanceRepository Attendance => attendance;

    public INotificationRepository Notifications => notifications;
    public INotificationPreferencesRepository NotificationPreferences => notificationPreferences;

    public IAuditLogRepository AuditLogs => auditLogs;

    public IDocumentRepository Documents => documents;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
