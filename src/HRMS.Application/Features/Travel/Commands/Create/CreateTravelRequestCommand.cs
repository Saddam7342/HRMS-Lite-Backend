using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Travel.Commands.Create;

public record CreateTravelRequestCommand : IRequest<Result<Guid>>
{
    public string Destination { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public decimal? EstimatedBudget { get; init; }
}

public class CreateTravelRequestValidator : AbstractValidator<CreateTravelRequestCommand>
{
    public CreateTravelRequestValidator()
    {
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FromDate).NotEmpty().LessThanOrEqualTo(x => x.ToDate);
        RuleFor(x => x.ToDate).NotEmpty();
        RuleFor(x => x.EstimatedBudget).GreaterThanOrEqualTo(0).When(x => x.EstimatedBudget.HasValue);
    }
}

public class CreateTravelRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateTravelRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<Guid>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result<Guid>.Failure("Employee profile not found.");

        // Overlap Check
        if (await unitOfWork.TravelRequests.HasOverlappingTravelAsync(employee.Id, request.FromDate, request.ToDate, null, cancellationToken))
            return Result<Guid>.Failure("You already have an overlapping travel request for these dates.");

        var travelRequest = new TravelRequest
        {
            EmployeeId = employee.Id,
            Destination = request.Destination,
            Purpose = request.Purpose,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            EstimatedBudget = request.EstimatedBudget,
            Status = TravelRequestStatus.Pending
        };

        await unitOfWork.TravelRequests.AddAsync(travelRequest, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(travelRequest.Id);
    }
}
