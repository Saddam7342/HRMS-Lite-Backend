using HRMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HRMS.Persistence.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var possiblePaths = new[] 
        { 
            Path.Combine(Directory.GetCurrentDirectory(), "src/HRMS.API"),
            Path.Combine(Directory.GetCurrentDirectory(), "../HRMS.API"),
            Directory.GetCurrentDirectory() 
        };

        string basePath = possiblePaths.FirstOrDefault(p => File.Exists(Path.Combine(p, "appsettings.json"))) ?? possiblePaths[0];
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        return new AppDbContext(builder.Options, new DesignTimeTenantContext());
    }
}

public class DesignTimeTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public string? TenantSlug => null;
    public bool IsResolved => true;
    public void SetTenant(Guid tenantId, string? slug = null) { }
}
