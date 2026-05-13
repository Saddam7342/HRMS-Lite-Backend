using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRMS.Persistence.Seeding;

/// <summary>
/// Seeds the database with the 3 roles (Admin, Manager, Employee),
/// their permissions, and the default company admin user.
/// Single-company HRMS — no organization or tenant seeding required.
/// </summary>
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
            await SeedAdminUserAsync(context, passwordHasher);
            await SeedAdminEmployeeIfMissingAsync(context);
            await SeedDefaultLeaveTypesAsync(context);
            await SeedDefaultExpenseCategoriesAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity data.");
        }
    }

    // ---------------------------------------------------------------
    // Roles & Permissions
    // ---------------------------------------------------------------
    private static async Task SeedRolesAndPermissionsAsync(AppDbContext context)
    {
        // Permissions — aligned to 3-role model
        var permissions = new List<Permission>
        {
            // Employees
            new() { Name = "View Employees",   Code = "employees:view",   Module = "Employees" },
            new() { Name = "Create Employee",  Code = "employees:create", Module = "Employees" },
            new() { Name = "Update Employee",  Code = "employees:update", Module = "Employees" },
            new() { Name = "Delete Employee",  Code = "employees:delete", Module = "Employees" },

            // Departments
            new() { Name = "View Departments",   Code = "departments:view",   Module = "Departments" },
            new() { Name = "Manage Departments", Code = "departments:manage", Module = "Departments" },

            // Leaves
            new() { Name = "View Leaves",   Code = "leaves:view",   Module = "Leaves" },
            new() { Name = "Apply Leave",   Code = "leaves:create", Module = "Leaves" },
            new() { Name = "Approve Leave", Code = "leaves:approve", Module = "Leaves" },

            // Expenses
            new() { Name = "View Expense Claims",   Code = "claims:view",    Module = "Expenses" },
            new() { Name = "Create Expense Claim",  Code = "claims:create",  Module = "Expenses" },
            new() { Name = "Approve Expense Claim", Code = "claims:approve", Module = "Expenses" },

            // Travel
            new() { Name = "View Travel Requests",   Code = "travel:view",    Module = "Travel" },
            new() { Name = "Create Travel Request",  Code = "travel:create",  Module = "Travel" },
            new() { Name = "Approve Travel Request", Code = "travel:approve", Module = "Travel" },

            // Attendance
            new() { Name = "Record Attendance", Code = "attendance:record", Module = "Attendance" },
            new() { Name = "View Attendance",   Code = "attendance:view",   Module = "Attendance" },
            new() { Name = "Manage Attendance", Code = "attendance:manage", Module = "Attendance" },

            // Documents
            new() { Name = "Upload Documents",        Code = "documents:upload", Module = "Documents" },
            new() { Name = "View All Documents",       Code = "documents:view",   Module = "Documents" },
            new() { Name = "Upload Company Documents", Code = "documents:company",Module = "Documents" },

            // Notifications
            new() { Name = "View Notifications", Code = "notifications:view", Module = "Notifications" },

            // Reports
            new() { Name = "View Reports", Code = "reports:view", Module = "Reports" },

            // Audit
            new() { Name = "View Audit Logs", Code = "audit:view", Module = "Audit" },
        };

        foreach (var p in permissions)
        {
            if (!await context.Permissions.AnyAsync(x => x.Code == p.Code))
                context.Permissions.Add(p);
        }
        await context.SaveChangesAsync();

        // Roles
        var admin    = await EnsureRoleAsync(context, "Admin",    "HR / Company Admin — full access");
        var manager  = await EnsureRoleAsync(context, "Manager",  "Team Manager — approve team requests");
        var employee = await EnsureRoleAsync(context, "Employee", "Standard employee — self-service");

        // Admin gets ALL permissions
        await AssignAllPermissionsAsync(context, admin.Id);

        // Manager permissions
        foreach (var code in new[]
        {
            "employees:view",
            "departments:view",
            "leaves:view", "leaves:approve",
            "claims:view", "claims:approve",
            "travel:view", "travel:approve",
            "attendance:record", "attendance:view",
            "documents:upload", "documents:view",
            "notifications:view",
            "reports:view"
        })
            await AssignPermissionAsync(context, manager.Id, code);

        // Employee permissions
        foreach (var code in new[]
        {
            "leaves:view", "leaves:create",
            "claims:view", "claims:create",
            "travel:view", "travel:create",
            "attendance:record", "attendance:view",
            "documents:upload",
            "notifications:view"
        })
            await AssignPermissionAsync(context, employee.Id, code);

        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    // Default Admin User
    // ---------------------------------------------------------------
    private static async Task SeedAdminUserAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        const string adminEmail = "admin@company.com";

        if (!await context.Users.AnyAsync(x => x.Email == adminEmail))
        {
            var admin = new AppUser
            {
                FirstName = "Company",
                LastName = "Admin",
                Email = adminEmail,
                Username = "admin",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                IsActive = true,
                IsEmailConfirmed = true,
                PasswordResetRequired = false
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstAsync(x => x.Name == "Admin");
            context.UserRoles.Add(new HRMS.Domain.Entities.UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Approvals and attendance team queries resolve the current user via <see cref="Employee"/> —
    /// ensure the default admin has an employee profile.
    /// </summary>
    private static async Task SeedAdminEmployeeIfMissingAsync(AppDbContext context)
    {
        const string adminEmail = "admin@company.com";
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == adminEmail);
        if (user == null) return;

        if (await context.Employees.AnyAsync(e => e.UserId == user.Id)) return;

        context.Employees.Add(new Employee
        {
            UserId = user.Id,
            EmployeeCode = "ADM001",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            HireDate = DateTime.UtcNow.Date,
            Designation = "Administrator",
            Status = EmployeeStatus.Active,
            IsActive = true,
        });
        await context.SaveChangesAsync();

        await SeedLeaveBalancesForAdminAsync(context, user.Id);
    }

    private static async Task SeedLeaveBalancesForAdminAsync(AppDbContext context, Guid userId)
    {
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        if (employee == null) return;

        var currentYear = DateTime.UtcNow.Year;
        var leaveTypes = await context.LeaveTypes.ToListAsync();

        foreach (var lt in leaveTypes)
        {
            if (!await context.LeaveBalances.AnyAsync(b => b.EmployeeId == employee.Id && b.LeaveTypeId == lt.Id && b.Year == currentYear))
            {
                context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = lt.Id,
                    TotalDays = lt.DefaultDays,
                    UsedDays = 0,
                    Year = currentYear
                });
            }
        }
        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    // Default Leave Types (company-wide, no TenantId)
    // ---------------------------------------------------------------
    private static async Task SeedDefaultLeaveTypesAsync(AppDbContext context)
    {
        var leaveTypes = new List<LeaveType>
        {
            new() { Name = "Casual Leave",    Code = "CL", DefaultDays = 12 },
            new() { Name = "Sick Leave",      Code = "SL", DefaultDays = 10 },
            new() { Name = "Annual Leave",    Code = "AL", DefaultDays = 15 },
            new() { Name = "Maternity Leave", Code = "ML", DefaultDays = 90, IsGenderSpecific = true, ApplicableGender = Gender.Female },
            new() { Name = "Paternity Leave", Code = "PL", DefaultDays = 7,  IsGenderSpecific = true, ApplicableGender = Gender.Male }
        };

        foreach (var lt in leaveTypes)
        {
            if (!await context.LeaveTypes.AnyAsync(x => x.Code == lt.Code))
                context.LeaveTypes.Add(lt);
        }
        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    // Default Expense Categories (company-wide, no TenantId)
    // ---------------------------------------------------------------
    private static async Task SeedDefaultExpenseCategoriesAsync(AppDbContext context)
    {
        var categories = new List<ExpenseCategory>
        {
            new() { Name = "Travel",          Code = "TRAVEL",  Description = "Business travel expenses" },
            new() { Name = "Food",            Code = "FOOD",    Description = "Meal reimbursements" },
            new() { Name = "Fuel",            Code = "FUEL",    Description = "Vehicle fuel" },
            new() { Name = "Accommodation",   Code = "STAY",    Description = "Hotel/lodging" },
            new() { Name = "Office Supplies", Code = "SUPPLY",  Description = "Stationery and equipment" },
            new() { Name = "Medical",         Code = "MED",     Description = "Health-related claims" },
            new() { Name = "Other",           Code = "OTHER",   Description = "Miscellaneous" }
        };

        foreach (var cat in categories)
        {
            if (!await context.ExpenseCategories.AnyAsync(x => x.Code == cat.Code))
                context.ExpenseCategories.Add(cat);
        }
        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
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

    private static async Task AssignAllPermissionsAsync(AppDbContext context, Guid roleId)
    {
        var all = await context.Permissions.ToListAsync();
        foreach (var p in all)
        {
            if (!await context.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == p.Id))
                context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = p.Id });
        }
    }

    private static async Task AssignPermissionAsync(AppDbContext context, Guid roleId, string permCode)
    {
        var perm = await context.Permissions.FirstOrDefaultAsync(p => p.Code == permCode);
        if (perm == null) return;
        if (!await context.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == perm.Id))
            context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id });
    }
}
