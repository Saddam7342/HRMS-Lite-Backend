$base = 'c:\Users\Technupur PC1\Documents\HRMS\src'

$files = @(
    "$base\HRMS.Domain\Entities\Organization.cs",
    "$base\HRMS.Domain\Entities\OrganizationSetting.cs",
    "$base\HRMS.Domain\Entities\Payroll.cs",
    "$base\HRMS.Domain\Common\TenantEntity.cs",
    "$base\HRMS.Domain\Common\Interfaces\ITenantEntity.cs",
    "$base\HRMS.Application\Common\Interfaces\ITenantContext.cs",
    "$base\HRMS.Application\Common\Interfaces\IOrganizationRepository.cs",
    "$base\HRMS.Application\Common\Interfaces\IOrganizationSettingRepository.cs",
    "$base\HRMS.Application\Common\Interfaces\IOrganizationProvisioningService.cs",
    "$base\HRMS.Application\Common\Interfaces\IPayrollEngine.cs",
    "$base\HRMS.Application\Common\Interfaces\IPayrollRepository.cs",
    "$base\HRMS.Application\Common\Interfaces\ISettingsService.cs",
    "$base\HRMS.Infrastructure\Services\OrganizationProvisioningService.cs",
    "$base\HRMS.Infrastructure\Services\PayrollEngine.cs",
    "$base\HRMS.Infrastructure\Services\SettingsService.cs",
    "$base\HRMS.API\Controllers\OrganizationsController.cs",
    "$base\HRMS.API\Controllers\PayrollController.cs",
    "$base\HRMS.API\Controllers\SettingsController.cs",
    "$base\HRMS.API\Middleware\TenantResolutionMiddleware.cs",
    "$base\HRMS.API\Middleware\FeatureFlagMiddleware.cs",
    "$base\HRMS.Persistence\Configurations\OrganizationConfiguration.cs",
    "$base\HRMS.Persistence\Configurations\OrganizationSettingConfiguration.cs",
    "$base\HRMS.Persistence\Configurations\PayrollConfiguration.cs",
    "$base\HRMS.Persistence\Repositories\OrganizationRepository.cs",
    "$base\HRMS.Persistence\Repositories\OrganizationSettingRepository.cs",
    "$base\HRMS.Persistence\Repositories\PayrollRepository.cs"
)

foreach ($f in $files) {
    if (Test-Path $f) {
        Remove-Item $f -Force
        Write-Host "DELETED: $f"
    } else {
        Write-Host "SKIP (not found): $f"
    }
}

$folders = @(
    "$base\HRMS.Application\Features\Organizations",
    "$base\HRMS.Application\Features\Payroll",
    "$base\HRMS.Application\Features\Platform",
    "$base\HRMS.Application\Features\Settings",
    "$base\HRMS.Infrastructure\Tenancy",
    "$base\HRMS.API\Controllers\Platform"
)

foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Remove-Item $folder -Recurse -Force
        Write-Host "DELETED FOLDER: $folder"
    } else {
        Write-Host "SKIP FOLDER (not found): $folder"
    }
}

Write-Host ""
Write-Host "Phase 1 deletions complete."
