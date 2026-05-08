namespace HRMS.Application.Features.Departments.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    Guid? DepartmentHeadId,
    string? DepartmentHeadName,
    bool IsActive);

public record DepartmentListDto(
    Guid Id,
    string Name,
    string Code,
    string? ParentDepartmentName,
    string? DepartmentHeadName,
    bool IsActive,
    int EmployeeCount);

public record DepartmentHierarchyDto(
    Guid Id,
    string Name,
    string Code,
    string? DepartmentHeadName,
    List<DepartmentHierarchyDto> Children);

public record DepartmentEmployeeDto(
    Guid Id,
    string FullName,
    string? Designation,
    string? ProfileImageUrl,
    bool IsHead);

public record CreateDepartmentRequest(
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? DepartmentHeadId);

public record UpdateDepartmentRequest(
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? DepartmentHeadId);
