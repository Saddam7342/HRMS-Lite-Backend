using AutoMapper;
using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Audit.DTOs;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Audit.Queries;

public record GetEntityAuditHistoryQuery(string EntityName, string EntityId) : IRequest<Result<IReadOnlyList<AuditLogDto>>>;
public record GetUserActivityHistoryQuery(Guid UserId, int Limit = 50) : IRequest<Result<IReadOnlyList<AuditLogDto>>>;
public record GetSystemAuditLogsQuery(int Page = 1, int PageSize = 50) : IRequest<Result<IReadOnlyList<AuditLogDto>>>;
public record GetAuditLogByIdQuery(Guid Id) : IRequest<Result<AuditLogDto>>;

public class AuditQueryHandlers(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService) 
    : IRequestHandler<GetEntityAuditHistoryQuery, Result<IReadOnlyList<AuditLogDto>>>,
      IRequestHandler<GetUserActivityHistoryQuery, Result<IReadOnlyList<AuditLogDto>>>,
      IRequestHandler<GetSystemAuditLogsQuery, Result<IReadOnlyList<AuditLogDto>>>,
      IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
{
    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetEntityAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        var logs = await unitOfWork.AuditLogs.GetQueryable()
            .Include(x => x.User)
            .Where(x => x.EntityName == request.EntityName && x.EntityId == request.EntityId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(mapper.Map<List<AuditLogDto>>(logs));
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetUserActivityHistoryQuery request, CancellationToken cancellationToken)
    {
        // Security check: Only Admins can view other users' activity
        if (currentUserService.UserId != request.UserId && !OrgRoles.IsCompanyAdmin(currentUserService.Roles))
            return Result<IReadOnlyList<AuditLogDto>>.Failure("Unauthorized.");

        var logs = await unitOfWork.AuditLogs.GetQueryable()
            .Include(x => x.User)
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(mapper.Map<List<AuditLogDto>>(logs));
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetSystemAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (!OrgRoles.IsCompanyAdmin(currentUserService.Roles))
            return Result<IReadOnlyList<AuditLogDto>>.Failure("Unauthorized.");

        var logs = await unitOfWork.AuditLogs.GetQueryable()
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AuditLogDto>>.Success(mapper.Map<List<AuditLogDto>>(logs));
    }

    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await unitOfWork.AuditLogs.GetQueryable()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (log == null) return Result<AuditLogDto>.Failure("Log not found.");
        
        return Result<AuditLogDto>.Success(mapper.Map<AuditLogDto>(log));
    }
}
