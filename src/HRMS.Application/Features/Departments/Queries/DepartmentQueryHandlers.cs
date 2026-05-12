using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Departments.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Departments.Queries;

public record GetDepartmentsQuery : IRequest<Result<IReadOnlyList<DepartmentListDto>>>;
public record GetDepartmentByIdQuery(Guid Id) : IRequest<Result<DepartmentDto>>;
public record GetDepartmentEmployeesQuery(Guid Id) : IRequest<Result<IReadOnlyList<DepartmentEmployeeDto>>>;
public record GetDepartmentHierarchyQuery : IRequest<Result<List<DepartmentHierarchyDto>>>;

public class DepartmentQueryHandlers(
    IUnitOfWork unitOfWork,
    IMapper mapper) 
    : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyList<DepartmentListDto>>>,
      IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>,
      IRequestHandler<GetDepartmentEmployeesQuery, Result<IReadOnlyList<DepartmentEmployeeDto>>>,
      IRequestHandler<GetDepartmentHierarchyQuery, Result<List<DepartmentHierarchyDto>>>
{
    public async Task<Result<IReadOnlyList<DepartmentListDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        try 
        {
            var departments = await unitOfWork.Departments.GetQueryable()
                .AsNoTracking()
                .Include(x => x.ParentDepartment)
                .Include(x => x.DepartmentHead)
                .Include(x => x.Employees)
                .ToListAsync(cancellationToken);

            // Map to List<> first — AutoMapper does not map List<Department> -> IReadOnlyList<Dto> directly.
            var dtos = mapper.Map<List<DepartmentListDto>>(departments);
            return Result<IReadOnlyList<DepartmentListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<DepartmentListDto>>.Failure($"Failed to retrieve departments: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentDto>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetWithDetailsAsync(request.Id, cancellationToken);
        if (department == null) return Result<DepartmentDto>.Failure("Department not found.");

        return Result<DepartmentDto>.Success(mapper.Map<DepartmentDto>(department));
    }

    public async Task<Result<IReadOnlyList<DepartmentEmployeeDto>>> Handle(GetDepartmentEmployeesQuery request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
        if (department == null) return Result<IReadOnlyList<DepartmentEmployeeDto>>.Failure("Department not found.");

        var employees = await unitOfWork.Employees.GetQueryable()
            .Where(x => x.DepartmentId == request.Id)
            .ToListAsync(cancellationToken);

        var dtos = employees.Select(e => new DepartmentEmployeeDto(
            e.Id,
            $"{e.FirstName} {e.LastName}",
            e.Designation,
            e.ProfileImageUrl,
            e.Id == department.DepartmentHeadId)).ToList();

        return Result<IReadOnlyList<DepartmentEmployeeDto>>.Success(dtos);
    }

    public async Task<Result<List<DepartmentHierarchyDto>>> Handle(GetDepartmentHierarchyQuery request, CancellationToken cancellationToken)
    {
        var departments = await unitOfWork.Departments.GetHierarchyAsync(cancellationToken);
        
        var hierarchy = BuildHierarchy(departments, null);
        return Result<List<DepartmentHierarchyDto>>.Success(hierarchy);
    }

    private List<DepartmentHierarchyDto> BuildHierarchy(IReadOnlyList<Department> items, Guid? parentId)
    {
        return items
            .Where(x => x.ParentDepartmentId == parentId)
            .Select(x => new DepartmentHierarchyDto(
                x.Id,
                x.Name,
                x.Code,
                x.DepartmentHead != null ? $"{x.DepartmentHead.FirstName} {x.DepartmentHead.LastName}" : null,
                BuildHierarchy(items, x.Id)
            ))
            .ToList();
    }
}
