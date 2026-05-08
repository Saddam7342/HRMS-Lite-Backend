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
using HRMS.Application.Features.Organizations.DTOs;
using HRMS.Application.Features.Payroll.DTOs;
using HRMS.Application.Features.Settings.DTOs;
using HRMS.Application.Features.Travel.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Organization
        CreateMap<Organization, OrganizationDto>();
        CreateMap<Organization, OrganizationBrandingDto>();

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.ManagerName, opt => opt.MapFrom(s => s.Manager != null ? $"{s.Manager.FirstName} {s.Manager.LastName}" : null));

        CreateMap<Employee, EmployeeListDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null));

        CreateMap<Employee, EmployeeProfileDto>()
            .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.ManagerName, opt => opt.MapFrom(s => s.Manager != null ? $"{s.Manager.FirstName} {s.Manager.LastName}" : null));

        CreateMap<Employee, TeamMemberDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"));

        // Department
        CreateMap<Department, DepartmentDto>()
            .ForMember(d => d.ParentDepartmentName, opt => opt.MapFrom(s => s.ParentDepartment != null ? s.ParentDepartment.Name : null))
            .ForMember(d => d.DepartmentHeadName, opt => opt.MapFrom(s => s.DepartmentHead != null ? $"{s.DepartmentHead.FirstName} {s.DepartmentHead.LastName}" : null));

        CreateMap<Department, DepartmentListDto>()
            .ForMember(d => d.ParentDepartmentName, opt => opt.MapFrom(s => s.ParentDepartment != null ? s.ParentDepartment.Name : null))
            .ForMember(d => d.DepartmentHeadName, opt => opt.MapFrom(s => s.DepartmentHead != null ? $"{s.DepartmentHead.FirstName} {s.DepartmentHead.LastName}" : null))
            .ForMember(d => d.EmployeeCount, opt => opt.MapFrom(s => s.Employees.Count));

        // Leave
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.LeaveTypeName, opt => opt.MapFrom(s => s.LeaveType.Name))
            .ForMember(d => d.ApproverName, opt => opt.MapFrom(s => s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null));

        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(d => d.LeaveTypeName, opt => opt.MapFrom(s => s.LeaveType.Name));

        CreateMap<LeaveRequest, LeaveCalendarDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.LeaveTypeName, opt => opt.MapFrom(s => s.LeaveType.Name));

        // Expense
        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        
        CreateMap<ExpenseClaim, ExpenseClaimDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.ApproverName, opt => opt.MapFrom(s => s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null));

        CreateMap<ExpenseClaim, ExpenseClaimListDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

        // Travel
        CreateMap<TravelRequest, TravelRequestDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.ApproverName, opt => opt.MapFrom(s => s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null));

        CreateMap<TravelRequest, TravelRequestListDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));

        CreateMap<TravelRequest, TeamTravelScheduleDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));

        // Attendance
        CreateMap<AttendanceRecord, AttendanceDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));

        CreateMap<AttendanceRecord, AttendanceListDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));

        // Notification
        CreateMap<Notification, NotificationDto>();
        CreateMap<NotificationPreferences, NotificationPreferencesDto>();

        // Audit
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "System"));

        // Settings
        CreateMap<OrganizationSetting, OrganizationSettingDto>();

        // Document
        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : null))
            .ForMember(d => d.UploadedByName, opt => opt.MapFrom(s => $"{s.UploadedBy.FirstName} {s.UploadedBy.LastName}"));

        // Payroll
        CreateMap<SalaryStructure, SalaryStructureDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.Allowances, opt => opt.MapFrom(s => JsonSerializer.Deserialize<List<AllowanceModel>>(s.Allowances, (JsonSerializerOptions)null!) ?? new List<AllowanceModel>()))
            .ForMember(d => d.Deductions, opt => opt.MapFrom(s => JsonSerializer.Deserialize<List<DeductionModel>>(s.Deductions, (JsonSerializerOptions)null!) ?? new List<DeductionModel>()));

        CreateMap<Payroll, PayrollDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.ApproverName, opt => opt.MapFrom(s => s.ApprovedBy != null ? $"{s.ApprovedBy.FirstName} {s.ApprovedBy.LastName}" : null));
    }
}
