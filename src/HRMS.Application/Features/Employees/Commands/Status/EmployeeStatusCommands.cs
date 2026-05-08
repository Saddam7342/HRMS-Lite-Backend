using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Employees.Commands.Status;

public record ActivateEmployeeCommand(Guid Id) : IRequest<Result>;
public record DeactivateEmployeeCommand(Guid Id) : IRequest<Result>;

public class EmployeeStatusHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<ActivateEmployeeCommand, Result>,
      IRequestHandler<DeactivateEmployeeCommand, Result>
{
    public async Task<Result> Handle(ActivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(request.Id, cancellationToken);
        if (employee == null) return Result.Failure("Employee not found.");

        employee.IsActive = true;
        employee.Status = EmployeeStatus.Active;
        employee.User.IsActive = true;

        unitOfWork.Employees.Update(employee);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Employee activated.");
    }

    public async Task<Result> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(request.Id, cancellationToken);
        if (employee == null) return Result.Failure("Employee not found.");

        employee.IsActive = false;
        employee.Status = EmployeeStatus.Inactive;
        employee.User.IsActive = false;

        unitOfWork.Employees.Update(employee);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Employee deactivated.");
    }
}
