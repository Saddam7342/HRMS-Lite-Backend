Set-Location "c:\Users\Technupur PC1\Documents\HRMS"

$domain      = "src/HRMS.Domain/HRMS.Domain.csproj"
$shared      = "src/HRMS.Shared/HRMS.Shared.csproj"
$application = "src/HRMS.Application/HRMS.Application.csproj"
$infra       = "src/HRMS.Infrastructure/HRMS.Infrastructure.csproj"
$persistence = "src/HRMS.Persistence/HRMS.Persistence.csproj"
$api         = "src/HRMS.API/HRMS.API.csproj"

Write-Host "=== Adding project references ===" -ForegroundColor Cyan

# Application references Domain + Shared
dotnet add $application reference $domain
dotnet add $application reference $shared

# Infrastructure references Application + Domain + Shared
dotnet add $infra reference $application
dotnet add $infra reference $domain
dotnet add $infra reference $shared

# Persistence references Application + Domain + Shared
dotnet add $persistence reference $application
dotnet add $persistence reference $domain
dotnet add $persistence reference $shared

# API references all
dotnet add $api reference $application
dotnet add $api reference $infra
dotnet add $api reference $persistence
dotnet add $api reference $shared
dotnet add $api reference $domain

Write-Host "=== Project references done ===" -ForegroundColor Green

Write-Host "=== Adding NuGet packages ===" -ForegroundColor Cyan

# ---- HRMS.Shared ----
dotnet add $shared package Microsoft.Extensions.DependencyInjection.Abstractions

# ---- HRMS.Domain ----
# Domain stays clean - no packages needed

# ---- HRMS.Application ----
dotnet add $application package MediatR
dotnet add $application package AutoMapper
dotnet add $application package FluentValidation
dotnet add $application package FluentValidation.DependencyInjectionExtensions
dotnet add $application package Microsoft.Extensions.Logging.Abstractions
dotnet add $application package Microsoft.Extensions.DependencyInjection.Abstractions

# ---- HRMS.Infrastructure ----
dotnet add $infra package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add $infra package Microsoft.IdentityModel.Tokens
dotnet add $infra package System.IdentityModel.Tokens.Jwt
dotnet add $infra package BCrypt.Net-Next
dotnet add $infra package Microsoft.Extensions.Configuration.Abstractions
dotnet add $infra package Microsoft.Extensions.Options
dotnet add $infra package Microsoft.Extensions.Http
dotnet add $infra package Microsoft.AspNetCore.Http.Abstractions

# ---- HRMS.Persistence ----
dotnet add $persistence package Microsoft.EntityFrameworkCore
dotnet add $persistence package Microsoft.EntityFrameworkCore.SqlServer
dotnet add $persistence package Microsoft.EntityFrameworkCore.Tools
dotnet add $persistence package Microsoft.EntityFrameworkCore.Design
dotnet add $persistence package Microsoft.Extensions.Configuration.Abstractions
dotnet add $persistence package Microsoft.Extensions.DependencyInjection.Abstractions

# ---- HRMS.API ----
dotnet add $api package Swashbuckle.AspNetCore
dotnet add $api package Asp.Versioning.Mvc
dotnet add $api package Asp.Versioning.ApiExplorer
dotnet add $api package Serilog.AspNetCore
dotnet add $api package Serilog.Sinks.Console
dotnet add $api package Serilog.Sinks.File
dotnet add $api package Serilog.Enrichers.Environment
dotnet add $api package Serilog.Enrichers.Thread
dotnet add $api package Microsoft.AspNetCore.Diagnostics.HealthChecks
dotnet add $api package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
dotnet add $api package AutoMapper

Write-Host "=== NuGet packages done ===" -ForegroundColor Green
Write-Host "=== Setup complete! ===" -ForegroundColor Yellow
