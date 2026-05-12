using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Departments.Commands.Update;

public record UpdateDepartmentCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public Guid? DepartmentHeadId { get; init; }
}

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        
        RuleFor(x => x).MustAsync(NotCreateCircularReference).WithMessage("Circular hierarchy reference detected.");
    }

    private async Task<bool> NotCreateCircularReference(UpdateDepartmentCommand command, CancellationToken ct)
    {
        if (!command.ParentDepartmentId.HasValue) return true;
        if (command.Id == command.ParentDepartmentId.Value) return false;

        // Traverse up to check if the new parent is a descendant of the current department
        var currentParentId = command.ParentDepartmentId;
        while (currentParentId.HasValue)
        {
            if (currentParentId == command.Id) return false;
            var parent = await _unitOfWork.Departments.GetByIdAsync(currentParentId.Value, ct);
            currentParentId = parent?.ParentDepartmentId;
        }

        return true;
    }
}

public class UpdateDepartmentHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateDepartmentCommand, Result>
{
    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
        if (department == null) return Result.Failure("Department not found.");

        department.Name = request.Name;
        department.Code = request.Code;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.DepartmentHeadId = request.DepartmentHeadId;

        unitOfWork.Departments.Update(department);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Department updated.");
    }
}
