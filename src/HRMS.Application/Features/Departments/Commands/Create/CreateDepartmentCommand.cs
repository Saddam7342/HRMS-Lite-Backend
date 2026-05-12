using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Departments.Commands.Create;

public record CreateDepartmentCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public Guid? DepartmentHeadId { get; init; }
}

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .MustAsync(BeUniqueName).WithMessage("Department name already exists.");
            
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .MustAsync(BeUniqueCode).WithMessage("Department code already exists.");

        RuleFor(x => x.ParentDepartmentId)
            .MustAsync(Exist).When(x => IsValidGuid(x.ParentDepartmentId))
            .WithMessage("Parent department not found.");

        RuleFor(x => x.DepartmentHeadId)
            .MustAsync(BeActiveEmployee).When(x => IsValidGuid(x.DepartmentHeadId))
            .WithMessage("Department head must be an active employee.");
    }

    private bool IsValidGuid(Guid? id) => 
        id.HasValue && id != Guid.Empty && id != Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private async Task<bool> BeUniqueName(string name, CancellationToken ct) =>
        !await _unitOfWork.Departments.NameExistsAsync(name, ct);

    private async Task<bool> BeUniqueCode(string code, CancellationToken ct) =>
        !await _unitOfWork.Departments.CodeExistsAsync(code, ct);

    private async Task<bool> Exist(Guid? id, CancellationToken ct)
    {
        var dept = await _unitOfWork.Departments.GetByIdAsync(id!.Value, ct);
        return dept != null;
    }

    private async Task<bool> BeActiveEmployee(Guid? id, CancellationToken ct)
    {
        var emp = await _unitOfWork.Employees.GetByIdAsync(id!.Value, ct);
        return emp != null && emp.IsActive;
    }
}

public class CreateDepartmentHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentDepartmentId = IsValidId(request.ParentDepartmentId) ? request.ParentDepartmentId : null,
            DepartmentHeadId = IsValidId(request.DepartmentHeadId) ? request.DepartmentHeadId : null,
            IsActive = true
        };

        await unitOfWork.Departments.AddAsync(department, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(department.Id);
    }

    private bool IsValidId(Guid? id) => 
        id.HasValue && id != Guid.Empty && id != Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
}
