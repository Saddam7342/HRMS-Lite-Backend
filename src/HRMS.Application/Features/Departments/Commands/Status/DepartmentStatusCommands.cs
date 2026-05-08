using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Departments.Commands.Status;

public record DeleteDepartmentCommand(Guid Id) : IRequest<Result>;
public record ActivateDepartmentCommand(Guid Id) : IRequest<Result>;
public record DeactivateDepartmentCommand(Guid Id) : IRequest<Result>;

public class DepartmentStatusHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteDepartmentCommand, Result>,
      IRequestHandler<ActivateDepartmentCommand, Result>,
      IRequestHandler<DeactivateDepartmentCommand, Result>
{
    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
        if (department == null) return Result.Failure("Department not found.");

        if (await unitOfWork.Departments.HasEmployeesAsync(request.Id, cancellationToken))
            return Result.Failure("Cannot delete department with assigned employees.");

        unitOfWork.Departments.Remove(department);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Department deleted.");
    }

    public async Task<Result> Handle(ActivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
        if (department == null) return Result.Failure("Department not found.");

        department.IsActive = true;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success("Department activated.");
    }

    public async Task<Result> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
        if (department == null) return Result.Failure("Department not found.");

        department.IsActive = false;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success("Department deactivated.");
    }
}
