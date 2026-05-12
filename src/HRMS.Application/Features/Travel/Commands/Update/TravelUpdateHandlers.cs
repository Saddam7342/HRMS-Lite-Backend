using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Travel.Commands.Update;

public record UpdateTravelRequestCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Destination { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public decimal? EstimatedBudget { get; init; }
}

public record CancelTravelRequestCommand(Guid Id) : IRequest<Result>;

public class TravelUpdateHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) 
    : IRequestHandler<UpdateTravelRequestCommand, Result>,
      IRequestHandler<CancelTravelRequestCommand, Result>
{
    public async Task<Result> Handle(UpdateTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travel = await unitOfWork.TravelRequests.GetByIdAsync(request.Id, cancellationToken);
        if (travel == null) return Result.Failure("Travel request not found.");

        if (travel.Status != TravelRequestStatus.Pending)
            return Result.Failure("Only pending requests can be modified.");

        var userId = currentUserService.UserId;
        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId ?? Guid.Empty, cancellationToken);
        if (employee == null || travel.EmployeeId != employee.Id)
            return Result.Failure("Unauthorized.");

        // Overlap Check (excluding current)
        if (await unitOfWork.TravelRequests.HasOverlappingTravelAsync(employee.Id, request.FromDate, request.ToDate, travel.Id, cancellationToken))
            return Result.Failure("Updated dates overlap with another approved travel request.");

        travel.Destination = request.Destination;
        travel.Purpose = request.Purpose;
        travel.FromDate = request.FromDate;
        travel.ToDate = request.ToDate;
        travel.EstimatedBudget = request.EstimatedBudget;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success("Travel request updated.");
    }

    public async Task<Result> Handle(CancelTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travel = await unitOfWork.TravelRequests.GetByIdAsync(request.Id, cancellationToken);
        if (travel == null) return Result.Failure("Travel request not found.");

        var userId = currentUserService.UserId;
        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId ?? Guid.Empty, cancellationToken);
        
        bool isOwner = employee != null && travel.EmployeeId == employee.Id;
        bool isOrgAdmin = OrgRoles.IsCompanyAdmin(currentUserService.Roles);

        if (!isOwner && !isOrgAdmin)
            return Result.Failure("Unauthorized.");

        if (travel.Status == TravelRequestStatus.Approved && !isOrgAdmin)
            return Result.Failure("Approved travel can only be cancelled by an administrator.");

        travel.Status = TravelRequestStatus.Cancelled;
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Travel request cancelled.");
    }
}
