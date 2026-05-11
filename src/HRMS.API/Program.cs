using System.Text;
using HRMS.Application;
using HRMS.Infrastructure;
using HRMS.Persistence;
using HRMS.Persistence.Context;
using HRMS.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Asp.Versioning;
using HRMS.Infrastructure.Settings;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using HRMS.Persistence.Seeding;
using HRMS.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog Configuration ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Add Layers ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// --- API Features ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "HRMS API", Version = "v1" });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// --- Authentication & Authorization ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? ""))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdminOnly", policy => policy.RequireRole("PlatformAdmin"));
    options.AddPolicy("OrgAdminOnly", policy => policy.RequireRole("OrganizationAdmin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    
    // Permission-based policy example
    options.AddPolicy("CanViewEmployees", policy => policy.RequireClaim(AppClaimTypes.Permission, "employees:view"));
});

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "SQL Server")
    .AddCheck("Memory Cache", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// --- Database Auto-Migration & Seeding ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // ONLY apply migrations automatically in Development OR if explicitly forced via env var
    var isDevelopment = app.Environment.IsDevelopment();
    var forceMigration = builder.Configuration.GetValue<bool>("ApplyMigrationsAtStartup", defaultValue: false);
    
    if (isDevelopment || forceMigration)
    {
        Log.Information("Applying migrations (Environment: {Env}, Forced: {Forced})...", app.Environment.EnvironmentName, forceMigration);
        int retryCount = 0;
        while (retryCount < 5)
        {
            try
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
                break;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                retryCount++;
                if (retryCount >= 5) 
                {
                    Log.Fatal(ex, "Database migration failed after multiple retries.");
                    throw;
                }
                Log.Warning("Database not ready, retrying in 5 seconds... (Attempt {RetryCount}/5)", retryCount);
                await Task.Delay(5000);
            }
        }
    }
    else
    {
        Log.Information("Auto-migration skipped in {Env} environment.", app.Environment.EnvironmentName);
    }
    
    try 
    {
        await IdentitySeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Seeding failed but application will continue to start.");
    }
}

// --- Middleware Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseStaticFiles();

// Recommended: Move HttpsRedirection higher or ensure it's not breaking health checks
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<FeatureFlagMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Optimized Health Check Output
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }
});

try
{
    Log.Information("Starting HRMS API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
