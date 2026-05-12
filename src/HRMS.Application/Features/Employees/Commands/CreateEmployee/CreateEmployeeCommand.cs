using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Events;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public DateTime HireDate { get; init; }
    public string? Designation { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? ManagerId { get; init; }
    public string? Address { get; init; }
}

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateEmployeeValidator(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255)
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists in this organization.");
            
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50)
            .MustAsync(BeUniqueCode).WithMessage("Employee code already exists.");

        RuleFor(x => x.HireDate).NotEmpty();
        RuleFor(x => x.Gender).IsInEnum();
        
        RuleFor(x => x).MustAsync(CheckSlotLimits).WithMessage("Organization employee limit reached.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        return !await _unitOfWork.Users.EmailExistsAsync(email, _tenantContext.TenantId, ct);
    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken ct)
    {
        return !await _unitOfWork.Employees.CodeExistsAsync(code, _tenantContext.TenantId, ct);
    }

    private async Task<bool> CheckSlotLimits(CreateEmployeeCommand command, CancellationToken ct)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(_tenantContext.TenantId, ct);
        if (org == null) return false;

        var currentCount = await _unitOfWork.Employees.GetCountByTenantAsync(_tenantContext.TenantId, ct);
        return currentCount < org.MaxEmployeeSlots;
    }
}

public class CreateEmployeeHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator) : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tempPassword = GenerateRandomPassword();
        var passwordHash = passwordHasher.HashPassword(tempPassword);

        try 
        {
            // 1. Create AppUser
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Email,
                PasswordHash = passwordHash,
                OrganizationId = tenantContext.TenantId,
                IsActive = true,
                IsEmailConfirmed = false,
                PasswordResetRequired = true
            };

            await unitOfWork.Users.AddAsync(user, cancellationToken);
            
            // 2. Create Employee
            var employee = new Employee
            {
                UserId = user.Id,
                TenantId = tenantContext.TenantId,
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                HireDate = request.HireDate,
                Designation = request.Designation,
                DepartmentId = request.DepartmentId,
                ManagerId = request.ManagerId,
                Address = request.Address,
                Status = EmployeeStatus.Active,
                IsActive = true
            };

            await unitOfWork.Employees.AddAsync(employee, cancellationToken);
            
            // 3. Assign Role (Using the Global Employee Role)
            var role = await unitOfWork.DbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == "Employee", cancellationToken);
            
            if (role != null)
            {
                unitOfWork.DbContext.UserRoles.Add(new HRMS.Domain.Entities.UserRole 
                { 
                    UserId = user.Id, 
                    RoleId = role.Id 
                });
            }

            // 4. Seed Leave Balances for all active leave types
            var leaveTypes = await unitOfWork.LeaveTypes.GetAllActiveAsync(tenantContext.TenantId, cancellationToken);
            var currentYear = dateTimeProvider.UtcNow.Year;

            foreach (var lt in leaveTypes)
            {
                // Skip gender-specific leaves that don't apply
                if (lt.IsGenderSpecific && lt.ApplicableGender != employee.Gender) continue;

                var balance = new LeaveBalance
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = lt.Id,
                    TotalDays = lt.DefaultDays,
                    UsedDays = 0,
                    Year = currentYear,
                    TenantId = tenantContext.TenantId
                };
                await unitOfWork.LeaveBalances.AddAsync(balance, cancellationToken);
            }

            // SINGLE COMMIT: Everything succeeds or everything fails
            await unitOfWork.CommitAsync(cancellationToken);

            // 5. Publish Event (This will trigger the Welcome Email)
            await mediator.Publish(new EmployeeCreatedEvent(employee, tempPassword), cancellationToken);

            return Result<Guid>.Success(employee.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Onboarding failed: {ex.Message}");
        }
    }

    private string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 10) + "1!Aa";
    }
}
