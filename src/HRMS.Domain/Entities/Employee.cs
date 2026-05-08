using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

/// <summary>
/// Employee profile linked to an AppUser.
/// Contains HR-specific data. Scoped to a tenant via TenantEntity.
/// </summary>
public class Employee : TenantEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public string? Designation { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    public bool IsActive { get; set; } = true;

    // FK relations
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    // Navigation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<ExpenseClaim> ExpenseClaims { get; set; } = [];
    public ICollection<TravelRequest> TravelRequests { get; set; } = [];
    public ICollection<Employee> DirectReports { get; set; } = [];
}
