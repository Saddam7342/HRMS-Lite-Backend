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
    private readonly ITenantContext _tenantContext;

    public CreateDepartmentValidator(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .MustAsync(BeUniqueName).WithMessage("Department name already exists.");
            
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .MustAsync(BeUniqueCode).WithMessage("Department code already exists.");

        RuleFor(x => x.ParentDepartmentId)
            .MustAsync(ExistAndBelongToTenant).When(x => x.ParentDepartmentId.HasValue)
            .WithMessage("Parent department not found.");

        RuleFor(x => x.DepartmentHeadId)
            .MustAsync(BeActiveEmployee).When(x => x.DepartmentHeadId.HasValue)
            .WithMessage("Department head must be an active employee.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct) =>
        !await _unitOfWork.Departments.NameExistsAsync(name, _tenantContext.TenantId, ct);

    private async Task<bool> BeUniqueCode(string code, CancellationToken ct) =>
        !await _unitOfWork.Departments.CodeExistsAsync(code, _tenantContext.TenantId, ct);

    private async Task<bool> ExistAndBelongToTenant(Guid? id, CancellationToken ct)
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

public class CreateDepartmentHandler(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId,
            DepartmentHeadId = request.DepartmentHeadId,
            TenantId = tenantContext.TenantId,
            IsActive = true
        };

        await unitOfWork.Departments.AddAsync(department, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(department.Id);
    }
}
