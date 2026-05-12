using System.Text.Json;
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
        // Employee
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

        CreateMap<Department, DepartmentListDto>()
            .ConstructUsing(s => new DepartmentListDto(
                s.Id,
                s.Name,
                s.Code,
                s.ParentDepartment != null ? s.ParentDepartment.Name : null,
                s.DepartmentHead != null ? $"{s.DepartmentHead.FirstName} {s.DepartmentHead.LastName}" : null,
                s.IsActive,
                s.Employees != null ? s.Employees.Count : 0));

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
                s.SubmittedAt,
                s.ApprovedById,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovalDate,
                s.RejectionReason));

        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ConstructUsing(s => new LeaveBalanceDto(
                s.Id,
                s.LeaveTypeId,
                s.LeaveType.Name,
                s.Year,
                s.TotalEntitlement,
                s.UsedDays,
                s.PendingDays,
                s.RemainingDays));

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
                s.Title,
                s.Description,
                s.CategoryId,
                s.Category.Name,
                s.Amount,
                s.Currency,
                s.ExpenseDate,
                s.ReceiptUrl,
                s.Status,
                s.SubmittedAt,
                s.ApprovedById,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovalDate,
                s.RejectionReason));

        CreateMap<ExpenseClaim, ExpenseClaimListDto>()
            .ConstructUsing(s => new ExpenseClaimListDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Title,
                s.CategoryId,
                s.Category.Name,
                s.Amount,
                s.Currency,
                s.ExpenseDate,
                s.Status,
                s.SubmittedAt));

        // Travel
        CreateMap<TravelRequest, TravelRequestDto>()
            .ConstructUsing(s => new TravelRequestDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Destination,
                s.FromDate,
                s.ToDate,
                s.Purpose,
                s.EstimatedBudget,
                s.TravelType,
                s.Status,
                s.SubmittedAt,
                s.ApprovedById,
                s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null,
                s.ApprovalDate,
                s.RejectionReason));

        CreateMap<TravelRequest, TravelRequestListDto>()
            .ConstructUsing(s => new TravelRequestListDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Destination,
                s.FromDate,
                s.ToDate,
                s.TravelType,
                s.Status,
                s.SubmittedAt));

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
                s.CheckIn,
                s.CheckOut,
                s.WorkHours,
                s.Status,
                s.Notes,
                s.Location,
                s.DeviceId));

        CreateMap<AttendanceRecord, AttendanceListDto>()
            .ConstructUsing(s => new AttendanceListDto(
                s.Id,
                s.EmployeeId,
                $"{s.Employee.FirstName} {s.Employee.LastName}",
                s.Date,
                s.CheckIn,
                s.CheckOut,
                s.WorkHours,
                s.Status));

        // Notification
        CreateMap<Notification, NotificationDto>();
        CreateMap<NotificationPreferences, NotificationPreferencesDto>();

        // Audit
        CreateMap<AuditLog, AuditLogDto>()
            .ConstructUsing(s => new AuditLogDto(
                s.Id,
                s.Action,
                s.TableName,
                s.RecordId,
                s.OldValues,
                s.NewValues,
                s.UserId,
                s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "System",
                s.Timestamp));

        // Document
        CreateMap<Document, DocumentDto>()
            .ConstructUsing(s => new DocumentDto(
                s.Id,
                s.Title,
                s.FileName,
                s.FilePath,
                s.FileSize,
                s.ContentType,
                s.Category,
                s.UploadedAt,
                s.UploadedById,
                $"{s.UploadedBy.FirstName} {s.UploadedBy.LastName}",
                s.EmployeeId,
                s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : null));
    }
}
