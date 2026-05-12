using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MediatR;

namespace HRMS.Infrastructure.Services;

public class OrganizationProvisioningService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IMediator mediator) : IOrganizationProvisioningService
{
    public async Task<Result<Guid>> ProvisionOrganizationAsync(
        string name,
        string slug,
        string adminEmail,
        int maxEmployeeSlots,
        CancellationToken ct = default)
    {
        // 1. Validate Uniqueness
        if (await unitOfWork.Organizations.GetBySlugAsync(slug, ct) != null)
            return Result<Guid>.Failure("An organization with this slug already exists.");

        if (await unitOfWork.Users.GetByEmailAsync(adminEmail, ct) != null)
            return Result<Guid>.Failure("An administrator with this email already exists.");

        // 2. Start Transaction (if supported by unit of work or use DB context directly)
        // Since IUnitOfWork doesn't explicitly expose transactions, we rely on CommitAsync 
        // after adding all entities. EF Core handles this as a single transaction by default.

        try
        {
            // 3. Create Organization
            var organization = new Organization
            {
                Name = name,
                Slug = slug.ToLower().Trim(),
                Email = adminEmail,
                MaxEmployeeSlots = maxEmployeeSlots,
                IsActive = true,
                TenantId = Guid.NewGuid() // Internal platform identifier
            };

            await unitOfWork.Organizations.AddAsync(organization, ct);
            await unitOfWork.CommitAsync(ct); // Save to get the ID

            // 4. Create Org Admin User
            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10) + "1!Aa"; 
            var adminUser = new AppUser
            {
                FirstName = "Admin",
                LastName = name,
                Email = adminEmail,
                Username = adminEmail,
                PasswordHash = passwordHasher.HashPassword(tempPassword),
                OrganizationId = organization.Id,
                IsActive = true,
                IsEmailConfirmed = true,
                PasswordResetRequired = true
            };

            await unitOfWork.Users.AddAsync(adminUser, ct);
            
            // 5. Assign OrganizationAdmin Role
            var orgAdminRole = await unitOfWork.DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "OrganizationAdmin", ct);
            if (orgAdminRole != null)
            {
                unitOfWork.DbContext.UserRoles.Add(new HRMS.Domain.Entities.UserRole { UserId = adminUser.Id, RoleId = orgAdminRole.Id });
            }

            // 6. Seed Default Data for the Tenant
            await SeedDefaultLeaveTypesAsync(organization.Id, ct);
            await SeedDefaultExpenseCategoriesAsync(organization.Id, ct);
            await SeedDefaultSettingsAsync(organization.Id, ct);

            await unitOfWork.CommitAsync(ct);

            // 7. Notify Org Admin
            await mediator.Publish(new HRMS.Domain.Events.OrganizationProvisionedEvent(organization, adminEmail, tempPassword), ct);

            return Result<Guid>.Success(organization.Id);
        }
        catch (Exception ex)
        {
            // Log error (should be done via ILogger)
            return Result<Guid>.Failure($"Provisioning failed: {ex.Message}");
        }
    }

    private async Task SeedDefaultLeaveTypesAsync(Guid tenantId, CancellationToken ct)
    {
        var leaveTypes = new List<LeaveType>
        {
            new() { Name = "Casual Leave", Code = "CL", DefaultDays = 12, TenantId = tenantId },
            new() { Name = "Sick Leave", Code = "SL", DefaultDays = 10, TenantId = tenantId },
            new() { Name = "Annual Leave", Code = "AL", DefaultDays = 15, TenantId = tenantId }
        };

        foreach (var lt in leaveTypes)
        {
            await unitOfWork.LeaveTypes.AddAsync(lt, ct);
        }
    }

    private async Task SeedDefaultExpenseCategoriesAsync(Guid tenantId, CancellationToken ct)
    {
        var categories = new List<ExpenseCategory>
        {
            new() { Name = "Travel", Code = "TRAVEL", Description = "Business travel expenses", TenantId = tenantId },
            new() { Name = "Food", Code = "FOOD", Description = "Meal reimbursements", TenantId = tenantId },
            new() { Name = "Other", Code = "OTHER", Description = "Miscellaneous", TenantId = tenantId }
        };

        foreach (var cat in categories)
        {
            await unitOfWork.ExpenseCategories.AddAsync(cat, ct);
        }
    }

    private async Task SeedDefaultSettingsAsync(Guid tenantId, CancellationToken ct)
    {
        var settings = new List<OrganizationSetting>
        {
            new() { Key = "features.leave.enabled", Value = "true", DataType = "bool", Description = "Enable Leave Module", TenantId = tenantId },
            new() { Key = "features.expense.enabled", Value = "true", DataType = "bool", Description = "Enable Expense Module", TenantId = tenantId },
            new() { Key = "features.attendance.enabled", Value = "true", DataType = "bool", Description = "Enable Attendance Module", TenantId = tenantId }
        };

        foreach (var s in settings)
        {
            await unitOfWork.Settings.AddAsync(s, ct);
        }
    }
}
