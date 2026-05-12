using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Events;

public record LeaveStatusChangedEvent(LeaveRequest LeaveRequest, string? Reason = null) : BaseEvent;
public record ExpenseStatusChangedEvent(ExpenseClaim ExpenseClaim, string? Reason = null) : BaseEvent;
public record TravelStatusChangedEvent(TravelRequest TravelRequest, string? Reason = null) : BaseEvent;
public record EmployeeCreatedEvent(Employee Employee, string TempPassword) : BaseEvent;
public record OrganizationProvisionedEvent(Organization Organization, string AdminEmail, string TempPassword) : BaseEvent;
public record PasswordResetRequestedEvent(AppUser User, string Token) : BaseEvent;
