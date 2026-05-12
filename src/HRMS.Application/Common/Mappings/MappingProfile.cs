using AutoMapper;
using HRMS.Application.Features.Attendance.DTOs;
using HRMS.Application.Features.Audit.DTOs;
using HRMS.Application.Features.Departments.DTOs;
using HRMS.Application.Features.Documents.DTOs;
using HRMS.Application.Features.Employees.DTOs;
using HRMS.Application.Features.Expenses.DTOs;
using HRMS.Application.Features.Leaves.DTOs;
using HRMS.Application.Features.Notifications.DTOs;
using HRMS.Application.Features.Travel.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Department
        CreateMap<Department, DepartmentDto>()
            .ConstructUsing(s => new DepartmentDto(
                s.Id,
                s.Name,
                s.Code,
                s.Description,
                s.ParentDepartmentId,
                s.ParentDepartment != null ? s.ParentDepartment.Name : null,
                s.DepartmentHeadId,
                s.DepartmentHead != null ? $"{s.DepartmentHead.FirstName} {s.DepartmentHead.LastName}" : null,
                s.IsActive));

        CreateMap<Department, DepartmentListDto>()
            .ConstructUsing(s => new DepartmentListDto(
                s.Id,
                s.Name,
                s.Code,
                s.ParentDepartment != null ? s.ParentDepartment.Name : null,
                s.DepartmentHead != null ? $"{s.DepartmentHead.FirstName} {s.DepartmentHead.LastName}" : null,
                s.IsActive,
                s.Employees != null ? s.Employees.Count : 0));

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ConstructUsing(s => new EmployeeDto(
                s.Id,
                s.EmployeeCode,
                s.FirstName,
                s.LastName,
                s.Email,
                s.PhoneNumber,
                s.Designation,
                s.Department != null ? s.Department.Name : null,
                s.Manager != null ? $"{s.Manager.FirstName} {s.Manager.LastName}" : null,
                s.Status,
                s.IsActive,
                s.ProfileImageUrl));

        CreateMap<Employee, EmployeeListDto>()
            .ConstructUsing(s => new EmployeeListDto(
                s.Id,
                s.EmployeeCode,
                $"{s.FirstName} {s.LastName}",
                s.Designation,
                s.Department != null ? s.Department.Name : null,
                s.Status,
                s.ProfileImageUrl));

        CreateMap<Employee, EmployeeProfileDto>()
            .ConstructUsing(s => new EmployeeProfileDto(
                s.Id,
                s.EmployeeCode,
                s.FirstName,
                s.LastName,
                s.Email,
                s.PhoneNumber,
                s.Gender,
                s.DateOfBirth,
                s.HireDate,
                s.Designation,
                s.DepartmentId,
                s.Department != null ? s.Department.Name : null,
                s.ManagerId,
                s.Manager != null ? $"{s.Manager.FirstName} {s.Manager.LastName}" : null,
                s.Status,
                s.Address,
                s.EmergencyContactName,
                s.EmergencyContactPhone,
                s.ProfileImageUrl));

        CreateMap<Employee, TeamMemberDto>()
            .ConstructUsing(s => new TeamMemberDto(
                s.Id,
                $"{s.FirstName} {s.LastName}",
                s.Designation,
                s.ProfileImageUrl,
                s.Status));

        // Leave
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ConstructUsing(s => new LeaveRequestDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.LeaveTypeId,
                s.LeaveType.Name,
                s.StartDate,
                s.EndDate,
                s.TotalDays,
                s.Reason,
                s.Status,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovedAt,
                s.RejectionReason));

        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ConstructUsing(s => new LeaveBalanceDto(
                s.LeaveTypeId,
                s.LeaveType.Name,
                s.TotalDays,
                s.UsedDays,
                s.RemainingDays,
                s.Year));

        CreateMap<LeaveRequest, LeaveCalendarDto>()
            .ConstructUsing(s => new LeaveCalendarDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.LeaveType.Name,
                s.StartDate,
                s.EndDate,
                s.Status));

        // Expense
        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        
        CreateMap<ExpenseClaim, ExpenseClaimDto>()
            .ConstructUsing(s => new ExpenseClaimDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.CategoryId,
                s.Category.Name,
                s.Title,
                s.Description,
                s.Amount,
                s.ExpenseDate,
                s.Status,
                s.ReceiptFileUrl,
                s.SubmittedAt,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovedAt,
                s.RejectionReason));

        CreateMap<ExpenseClaim, ExpenseClaimListDto>()
            .ConstructUsing(s => new ExpenseClaimListDto(
                s.Id,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Category.Name,
                s.Title,
                s.Amount,
                s.ExpenseDate,
                s.Status));

        // Travel
        CreateMap<TravelRequest, TravelRequestDto>()
            .ConstructUsing(s => new TravelRequestDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Destination,
                s.Purpose,
                s.FromDate,
                s.ToDate,
                s.Status,
                s.EstimatedBudget,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovedAt,
                s.RejectionReason));

        CreateMap<TravelRequest, TravelRequestListDto>()
            .ConstructUsing(s => new TravelRequestListDto(
                s.Id,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Destination,
                s.FromDate,
                s.ToDate,
                s.Status));

        CreateMap<TravelRequest, TeamTravelScheduleDto>()
            .ConstructUsing(s => new TeamTravelScheduleDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Destination,
                s.FromDate,
                s.ToDate,
                s.Status));

        // Attendance
        CreateMap<AttendanceRecord, AttendanceDto>()
            .ConstructUsing(s => new AttendanceDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Date,
                s.CheckInTime,
                s.CheckOutTime,
                s.TotalHours,
                s.Status,
                s.IsLate,
                s.Notes));

        CreateMap<AttendanceRecord, AttendanceListDto>()
            .ConstructUsing(s => new AttendanceListDto(
                s.Id,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Date,
                s.CheckInTime,
                s.CheckOutTime,
                s.Status));

        // Notification
        CreateMap<Notification, NotificationDto>();
        CreateMap<NotificationPreferences, NotificationPreferencesDto>();

        // Audit
        CreateMap<AuditLog, AuditLogDto>()
            .ConstructUsing(s => new AuditLogDto(
                s.Id,
                s.UserId,
                s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "System",
                s.ActionType,
                s.EntityName,
                s.EntityId,
                s.OldValues,
                s.NewValues,
                s.IpAddress,
                s.UserAgent,
                s.CreatedAt));

        // Document
        CreateMap<Document, DocumentDto>()
            .ConstructUsing(s => new DocumentDto(
                s.Id,
                s.Title,
                s.Description,
                s.FileName,
                s.FileType,
                s.FileSize,
                s.DocumentType,
                s.Category,
                s.EmployeeId,
                s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : null,
                s.UploadedById,
                s.UploadedBy != null ? $"{s.UploadedBy.FirstName} {s.UploadedBy.LastName}" : "System",
                s.Version,
                s.CreatedAt));
    }
}
