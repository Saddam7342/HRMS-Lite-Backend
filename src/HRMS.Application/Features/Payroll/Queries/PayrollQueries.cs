using System.Text.Json;
using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Payroll.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Payroll.Queries;

public record GetPayrollByMonthQuery(int Month, int Year) : IRequest<Result<IReadOnlyList<PayrollDto>>>;
public record GetMyPayslipQuery(int Month, int Year) : IRequest<Result<PaySlipDto>>;
public record GetPayrollSummaryQuery(int Month, int Year) : IRequest<Result<PayrollSummaryDto>>;

public class PayrollQueryHandlers(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService,
    IMapper mapper) 
    : IRequestHandler<GetPayrollByMonthQuery, Result<IReadOnlyList<PayrollDto>>>,
      IRequestHandler<GetMyPayslipQuery, Result<PaySlipDto>>,
      IRequestHandler<GetPayrollSummaryQuery, Result<PayrollSummaryDto>>
{
    public async Task<Result<IReadOnlyList<PayrollDto>>> Handle(GetPayrollByMonthQuery request, CancellationToken cancellationToken)
    {
        var payrolls = await unitOfWork.Payroll.GetMonthlyPayrollAsync(tenantContext.TenantId, request.Month, request.Year, cancellationToken);
        return Result<IReadOnlyList<PayrollDto>>.Success(mapper.Map<IReadOnlyList<PayrollDto>>(payrolls));
    }

    public async Task<Result<PaySlipDto>> Handle(GetMyPayslipQuery request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId ?? Guid.Empty, cancellationToken);
        if (employee == null) return Result<PaySlipDto>.Failure("Employee record not found.");

        var payroll = await unitOfWork.Payroll.GetByEmployeeAsync(employee.Id, request.Month, request.Year, cancellationToken);
        if (payroll == null) return Result<PaySlipDto>.Failure("Payslip not found for the selected period.");

        var payslip = new PaySlipDto(
            payroll.Id,
            $"{employee.FirstName} {employee.LastName}",
            employee.Designation ?? "N/A",
            employee.Department?.Name ?? "N/A",
            payroll.Month,
            payroll.Year,
            payroll.BasicSalary,
            JsonSerializer.Deserialize<List<AllowanceModel>>(payroll.AllowanceBreakdown) ?? [],
            JsonSerializer.Deserialize<List<DeductionModel>>(payroll.DeductionBreakdown) ?? [],
            payroll.NetSalary,
            payroll.GeneratedAt
        );

        return Result<PaySlipDto>.Success(payslip);
    }

    public async Task<Result<PayrollSummaryDto>> Handle(GetPayrollSummaryQuery request, CancellationToken cancellationToken)
    {
        var payrolls = await unitOfWork.Payroll.GetMonthlyPayrollAsync(tenantContext.TenantId, request.Month, request.Year, cancellationToken);
        if (!payrolls.Any()) return Result<PayrollSummaryDto>.Success(new PayrollSummaryDto(0, 0, 0, 0, 0, 0, 0));

        var summary = new PayrollSummaryDto(
            payrolls.Count,
            payrolls.Sum(x => x.BasicSalary),
            payrolls.Sum(x => x.TotalAllowances),
            payrolls.Sum(x => x.TotalDeductions),
            payrolls.Sum(x => x.NetSalary),
            payrolls.Count(x => x.Status == PayrollStatus.Approved),
            payrolls.Count(x => x.Status == PayrollStatus.Generated)
        );

        return Result<PayrollSummaryDto>.Success(summary);
    }
}
