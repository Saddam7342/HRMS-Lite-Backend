using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using HRMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRMS.Persistence.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await SeedRolesAndPermissionsAsync(context);
            await SeedPlatformAdminAsync(context, passwordHasher);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity data.");
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(AppDbContext context)
    {
        // Define Permissions
        var permissions = new List<Permission>
        {
            new() { Name = "View Organizations", Code = "orgs:view", Module = "Organizations" },
            new() { Name = "Create Organization", Code = "orgs:create", Module = "Organizations" },
            new() { Name = "Update Organization", Code = "orgs:update", Module = "Organizations" },
            new() { Name = "Delete Organization", Code = "orgs:delete", Module = "Organizations" },
            
            new() { Name = "View Employees", Code = "employees:view", Module = "Employees" },
            new() { Name = "Create Employee", Code = "employees:create", Module = "Employees" },
            new() { Name = "Update Employee", Code = "employees:update", Module = "Employees" },
            new() { Name = "Delete Employee", Code = "employees:delete", Module = "Employees" },

            new() { Name = "View Departments", Code = "departments:view", Module = "Departments" },
            new() { Name = "Manage Departments", Code = "departments:manage", Module = "Departments" },

            new() { Name = "View Leaves", Code = "leaves:view", Module = "Leaves" },
            new() { Name = "Apply Leave", Code = "leaves:create", Module = "Leaves" },
            new() { Name = "Approve Leave", Code = "leaves:approve", Module = "Leaves" },
            new() { Name = "Manage Leave Policies", Code = "leaves:manage", Module = "Leaves" },

            new() { Name = "View Expense Claims", Code = "claims:view", Module = "Expenses" },
            new() { Name = "Create Expense Claim", Code = "claims:create", Module = "Expenses" },
            new() { Name = "Approve Expense Claim", Code = "claims:approve", Module = "Expenses" },
            new() { Name = "Manage Expense Settings", Code = "claims:manage", Module = "Expenses" },

            new() { Name = "View Travel Requests", Code = "travel:view", Module = "Travel" },
            new() { Name = "Apply Travel", Code = "travel:create", Module = "Travel" },
            new() { Name = "Approve Travel", Code = "travel:approve", Module = "Travel" },
            new() { Name = "Manage Travel Settings", Code = "travel:manage", Module = "Travel" },

            new() { Name = "Attendance Check-In/Out", Code = "attendance:record", Module = "Attendance" },
            new() { Name = "View Attendance", Code = "attendance:view", Module = "Attendance" },
            new() { Name = "Manage Attendance", Code = "attendance:manage", Module = "Attendance" },

            new() { Name = "View Notifications", Code = "notifications:view", Module = "Notifications" },
            new() { Name = "Manage Notifications", Code = "notifications:manage", Module = "Notifications" },

            new() { Name = "View Settings", Code = "settings:view", Module = "Settings" },
            new() { Name = "Manage Settings", Code = "settings:manage", Module = "Settings" },
            new() { Name = "View Audit Logs", Code = "audit:view", Module = "Audit" }
        };

        foreach (var p in permissions)
        {
            if (!await context.Permissions.AnyAsync(x => x.Code == p.Code))
            {
                context.Permissions.Add(p);
            }
        }
        await context.SaveChangesAsync();

        // Define Roles
        var platformAdmin = await EnsureRoleAsync(context, "PlatformAdmin", "Full system access");
        var orgAdmin = await EnsureRoleAsync(context, "OrganizationAdmin", "Tenant-level full access");
        var manager = await EnsureRoleAsync(context, "Manager", "Department management");
        var employee = await EnsureRoleAsync(context, "Employee", "Standard user access");

        // Assign Permissions
        await AssignAllPermissionsAsync(context, platformAdmin.Id);
        await AssignAllPermissionsAsync(context, orgAdmin.Id);
        
        // Manager specific permissions
        await AssignPermissionAsync(context, manager.Id, "employees:view");
        await AssignPermissionAsync(context, manager.Id, "departments:view");
        await AssignPermissionAsync(context, manager.Id, "leaves:view");
        await AssignPermissionAsync(context, manager.Id, "leaves:approve");
        await AssignPermissionAsync(context, manager.Id, "claims:view");
        await AssignPermissionAsync(context, manager.Id, "claims:approve");
        await AssignPermissionAsync(context, manager.Id, "travel:view");
        await AssignPermissionAsync(context, manager.Id, "travel:approve");
        await AssignPermissionAsync(context, manager.Id, "attendance:record");
        await AssignPermissionAsync(context, manager.Id, "attendance:view");
        await AssignPermissionAsync(context, manager.Id, "notifications:view");
        
        // Employee specific permissions
        await AssignPermissionAsync(context, employee.Id, "leaves:view");
        await AssignPermissionAsync(context, employee.Id, "leaves:create");
        await AssignPermissionAsync(context, employee.Id, "claims:view");
        await AssignPermissionAsync(context, employee.Id, "claims:create");
        await AssignPermissionAsync(context, employee.Id, "travel:view");
        await AssignPermissionAsync(context, employee.Id, "travel:create");
        await AssignPermissionAsync(context, employee.Id, "attendance:record");
        await AssignPermissionAsync(context, employee.Id, "attendance:view");
        await AssignPermissionAsync(context, employee.Id, "notifications:view");
        
        await context.SaveChangesAsync();
    }

    private static async Task AssignAllPermissionsAsync(AppDbContext context, Guid roleId)
    {
        var allPermissions = await context.Permissions.ToListAsync();
        foreach (var p in allPermissions)
        {
            if (!await context.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == p.Id))
            {
                context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = p.Id });
            }
        }
    }

    private static async Task AssignPermissionAsync(AppDbContext context, Guid roleId, string permCode)
    {
        var perm = await context.Permissions.FirstAsync(p => p.Code == permCode);
        if (!await context.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == perm.Id))
        {
            context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id });
        }
    }

    private static async Task<Role> EnsureRoleAsync(AppDbContext context, string name, string desc)
    {
        var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == name);
        if (role == null)
        {
            role = new Role { Name = name, Description = desc };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }
        return role;
    }

    private static async Task SeedPlatformAdminAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        var systemOrg = await context.Organizations.FirstOrDefaultAsync(x => x.Slug == "system");
        if (systemOrg == null)
        {
            systemOrg = new Organization
            {
                Name = "System Administration",
                Slug = "system",
                Email = "admin@hrms-lite.com",
                IsActive = true,
                TenantId = Guid.NewGuid()
            };
            context.Organizations.Add(systemOrg);
            await context.SaveChangesAsync();

            // Seed Defaults for System Org
            await SeedDefaultLeaveTypesAsync(context, systemOrg.Id);
            await SeedDefaultExpenseCategoriesAsync(context, systemOrg.Id);
            await SeedDefaultSettingsAsync(context, systemOrg.Id);
        }

        var adminEmail = "admin@hrms-lite.com";
        if (!await context.Users.AnyAsync(x => x.Email == adminEmail))
        {
            var admin = new AppUser
            {
                FirstName = "Platform",
                LastName = "Admin",
                Email = adminEmail,
                Username = "admin",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                IsActive = true,
                IsEmailConfirmed = true,
                OrganizationId = systemOrg.Id
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            var role = await context.Roles.FirstAsync(x => x.Name == "PlatformAdmin");
            context.UserRoles.Add(new HRMS.Domain.Entities.UserRole { UserId = admin.Id, RoleId = role.Id });
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDefaultLeaveTypesAsync(AppDbContext context, Guid tenantId)
    {
        var leaveTypes = new List<LeaveType>
        {
            new() { Name = "Casual Leave", Code = "CL", DefaultDays = 12, TenantId = tenantId },
            new() { Name = "Sick Leave", Code = "SL", DefaultDays = 10, TenantId = tenantId },
            new() { Name = "Annual Leave", Code = "AL", DefaultDays = 15, TenantId = tenantId },
            new() { Name = "Maternity Leave", Code = "ML", DefaultDays = 90, IsGenderSpecific = true, ApplicableGender = Gender.Female, TenantId = tenantId },
            new() { Name = "Paternity Leave", Code = "PL", DefaultDays = 7, IsGenderSpecific = true, ApplicableGender = Gender.Male, TenantId = tenantId }
        };

        foreach (var lt in leaveTypes)
        {
            if (!await context.LeaveTypes.AnyAsync(x => x.Code == lt.Code && x.TenantId == tenantId))
            {
                context.LeaveTypes.Add(lt);
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultExpenseCategoriesAsync(AppDbContext context, Guid tenantId)
    {
        var categories = new List<ExpenseCategory>
        {
            new() { Name = "Travel", Code = "TRAVEL", Description = "Business travel expenses", TenantId = tenantId },
            new() { Name = "Food", Code = "FOOD", Description = "Meal reimbursements", TenantId = tenantId },
            new() { Name = "Fuel", Code = "FUEL", Description = "Vehicle fuel", TenantId = tenantId },
            new() { Name = "Accommodation", Code = "STAY", Description = "Hotel/lodging", TenantId = tenantId },
            new() { Name = "Office Supplies", Code = "SUPPLY", Description = "Stationery and equipment", TenantId = tenantId },
            new() { Name = "Medical", Code = "MED", Description = "Health-related claims", TenantId = tenantId },
            new() { Name = "Other", Code = "OTHER", Description = "Miscellaneous", TenantId = tenantId }
        };

        foreach (var cat in categories)
        {
            if (!await context.ExpenseCategories.AnyAsync(x => x.Code == cat.Code && x.TenantId == tenantId))
            {
                context.ExpenseCategories.Add(cat);
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultSettingsAsync(AppDbContext context, Guid tenantId)
    {
        var settings = new List<OrganizationSetting>
        {
            // Feature Toggles
            new() { Key = "features.leave.enabled", Value = "true", DataType = "bool", Description = "Enable Leave Management Module", TenantId = tenantId },
            new() { Key = "features.expense.enabled", Value = "true", DataType = "bool", Description = "Enable Expense Claim Module", TenantId = tenantId },
            new() { Key = "features.travel.enabled", Value = "true", DataType = "bool", Description = "Enable Travel Request Module", TenantId = tenantId },
            new() { Key = "features.attendance.enabled", Value = "true", DataType = "bool", Description = "Enable Attendance Module", TenantId = tenantId },

            // Leave Rules
            new() { Key = "leave.policy", Value = JsonSerializer.Serialize(new LeavePolicySettings()), DataType = "json", Description = "General Leave Policy Rules", TenantId = tenantId },

            // Attendance Rules
            new() { Key = "attendance.settings", Value = JsonSerializer.Serialize(new AttendanceSettings()), DataType = "json", Description = "Attendance & Working Hours Settings", TenantId = tenantId },

            // Expense Rules
            new() { Key = "expense.settings", Value = JsonSerializer.Serialize(new ExpenseSettings()), DataType = "json", Description = "Expense Reimbursement Limits", TenantId = tenantId },

            // Travel Rules
            new() { Key = "travel.settings", Value = JsonSerializer.Serialize(new TravelSettings()), DataType = "json", Description = "Travel Booking & Approval Rules", TenantId = tenantId },

            // General Org Settings
            new() { Key = "org.general", Value = JsonSerializer.Serialize(new OrganizationGeneralSettings()), DataType = "json", Description = "Organization General Settings", TenantId = tenantId }
        };

        foreach (var s in settings)
        {
            if (!await context.OrganizationSettings.AnyAsync(x => x.Key == s.Key && x.TenantId == tenantId))
            {
                context.OrganizationSettings.Add(s);
            }
        }
        await context.SaveChangesAsync();
    }
}
