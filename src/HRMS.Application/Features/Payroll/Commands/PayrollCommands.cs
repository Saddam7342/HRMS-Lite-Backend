using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Payroll.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Payroll.Commands;

public record CreateSalaryStructureCommand(CreateSalaryStructureRequest Request) : IRequest<Result<Guid>>;

public record GeneratePayrollCommand(int Month, int Year) : IRequest<Result<int>>;

public record ApprovePayrollCommand(Guid PayrollId) : IRequest<Result>;

public class PayrollCommandHandlers(
    IUnitOfWork unitOfWork,
    IPayrollEngine payrollEngine,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) 
    : IRequestHandler<CreateSalaryStructureCommand, Result<Guid>>,
      IRequestHandler<GeneratePayrollCommand, Result<int>>,
      IRequestHandler<ApprovePayrollCommand, Result>
{
    public async Task<Result<Guid>> Handle(CreateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Payroll.GetSalaryStructureAsync(request.Request.EmployeeId, cancellationToken);
        if (existing != null) return Result<Guid>.Failure("Salary structure already exists for this employee.");

        var structure = new SalaryStructure
        {
            EmployeeId = request.Request.EmployeeId,
            BasicSalary = request.Request.BasicSalary,
            Allowances = JsonSerializer.Serialize(request.Request.Allowances),
            Deductions = JsonSerializer.Serialize(request.Request.Deductions),
            OvertimeRatePerHour = request.Request.OvertimeRatePerHour,
            EffectiveFrom = request.Request.EffectiveFrom,
            TenantId = tenantContext.TenantId
        };

        await unitOfWork.DbContext.SalaryStructures.AddAsync(structure, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(structure.Id);
    }

    public async Task<Result<int>> Handle(GeneratePayrollCommand request, CancellationToken cancellationToken)
    {
        // 1. Get all active salary structures for tenant
        var structures = await unitOfWork.Payroll.GetAllSalaryStructuresAsync(tenantContext.TenantId, cancellationToken);
        if (!structures.Any()) return Result<int>.Failure("No salary structures found for the organization.");

        int count = 0;
        foreach (var structure in structures)
        {
            // 2. Check if already generated
            var existing = await unitOfWork.Payroll.GetByEmployeeAsync(structure.EmployeeId, request.Month, request.Year, cancellationToken);
            if (existing != null) continue;

            // 3. Calculate
            var payroll = await payrollEngine.CalculateMonthlyPayrollAsync(structure, request.Month, request.Year, cancellationToken);
            
            await unitOfWork.Payroll.AddAsync(payroll, cancellationToken);
            count++;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result<int>.Success(count);
    }

    public async Task<Result> Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
    {
        var payroll = await unitOfWork.Payroll.GetByIdAsync(request.PayrollId, cancellationToken);
        if (payroll == null) return Result.Failure("Payroll record not found.");
        if (payroll.Status == PayrollStatus.Approved) return Result.Failure("Payroll is already approved.");

        payroll.Status = PayrollStatus.Approved;
        payroll.ApprovedById = currentUserService.UserId;
        payroll.ApprovedAt = dateTimeProvider.UtcNow;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
