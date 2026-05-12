using AutoMapper;
using HRMS.Application.Common.Extensions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Employees.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Employees.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeProfileDto>>;
public record GetEmployeesQuery(PaginationParams Params) : IRequest<Result<PagedResult<EmployeeListDto>>>;
public record GetMyProfileQuery : IRequest<Result<EmployeeProfileDto>>;
public record GetTeamMembersQuery(Guid ManagerId) : IRequest<Result<IReadOnlyList<TeamMemberDto>>>;

public class EmployeeQueryHandlers(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService) 
    : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeProfileDto>>,
      IRequestHandler<GetEmployeesQuery, Result<PagedResult<EmployeeListDto>>>,
      IRequestHandler<GetMyProfileQuery, Result<EmployeeProfileDto>>,
      IRequestHandler<GetTeamMembersQuery, Result<IReadOnlyList<TeamMemberDto>>>
{
    public async Task<Result<EmployeeProfileDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(request.Id, cancellationToken);
        if (employee == null) return Result<EmployeeProfileDto>.Failure("Employee not found.");

        return Result<EmployeeProfileDto>.Success(mapper.Map<EmployeeProfileDto>(employee));
    }

    public async Task<Result<PagedResult<EmployeeListDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Employees.GetQueryable()
            .Include(x => x.Department)
            .OrderBy(x => x.LastName);

        var paged = await query.ToPagedResultAsync(request.Params.PageNumber, request.Params.PageSize, cancellationToken);
        var dtos = mapper.Map<List<EmployeeListDto>>(paged.Items);
        
        return Result<PagedResult<EmployeeListDto>>.Success(PagedResult<EmployeeListDto>.Create(
            dtos, paged.TotalCount, paged.PageNumber, paged.PageSize));
    }

    public async Task<Result<EmployeeProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<EmployeeProfileDto>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result<EmployeeProfileDto>.Failure("Profile not found.");

        // Eager load details
        var detailed = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(employee.Id, cancellationToken);
        return Result<EmployeeProfileDto>.Success(mapper.Map<EmployeeProfileDto>(detailed!));
    }

    public async Task<Result<IReadOnlyList<TeamMemberDto>>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var reports = await unitOfWork.Employees.GetDirectReportsAsync(request.ManagerId, cancellationToken);
        return Result<IReadOnlyList<TeamMemberDto>>.Success(mapper.Map<List<TeamMemberDto>>(reports));
    }
}
