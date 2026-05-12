using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HRMS.Persistence.Context;

/// <summary>
/// Design-time factory for EF Core migrations (dotnet ef migrations add ...).
/// Single-company HRMS — no ITenantContext needed.
/// </summary>
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

        string basePath = possiblePaths.FirstOrDefault(p => File.Exists(Path.Combine(p, "appsettings.json")))
                          ?? possiblePaths[0];

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new AppDbContext(builder.Options);
    }
}
