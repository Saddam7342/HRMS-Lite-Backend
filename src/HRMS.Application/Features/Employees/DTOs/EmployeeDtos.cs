using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Employees.DTOs;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Designation,
    string? DepartmentName,
    string? ManagerName,
    EmployeeStatus Status,
    bool IsActive,
    string? ProfileImageUrl);

public record EmployeeListDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string? Designation,
    string? DepartmentName,
    EmployeeStatus Status,
    string? ProfileImageUrl);

public record EmployeeProfileDto(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Gender Gender,
    DateTime DateOfBirth,
    DateTime HireDate,
    string? Designation,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? ManagerId,
    string? ManagerName,
    EmployeeStatus Status,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? ProfileImageUrl);

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string EmployeeCode,
    string? PhoneNumber,
    Gender Gender,
    DateTime DateOfBirth,
    DateTime HireDate,
    string? Designation,
    Guid? DepartmentId,
    Guid? ManagerId,
    string? Address);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Gender Gender,
    DateTime DateOfBirth,
    string? Designation,
    Guid? DepartmentId,
    Guid? ManagerId,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone);
    
public record TeamMemberDto(
    Guid Id,
    string FullName,
    string? Designation,
    string? ProfileImageUrl,
    EmployeeStatus Status);
