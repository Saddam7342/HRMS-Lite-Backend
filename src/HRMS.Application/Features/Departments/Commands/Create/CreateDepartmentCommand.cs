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
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct) =>
        !await _unitOfWork.Departments.NameExistsAsync(name, ct);

    private async Task<bool> BeUniqueCode(string code, CancellationToken ct) =>
        !await _unitOfWork.Departments.CodeExistsAsync(code, ct);
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
            ParentDepartmentId = null,
            DepartmentHeadId = null,
            IsActive = true
        };

        await unitOfWork.Departments.AddAsync(department, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(department.Id);
    }
}
