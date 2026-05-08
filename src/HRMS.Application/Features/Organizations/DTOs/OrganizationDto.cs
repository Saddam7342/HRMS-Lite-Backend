namespace HRMS.Application.Features.Organizations.DTOs;

public record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? LogoUrl,
    string? PrimaryColor,
    string? SecondaryColor,
    int MaxEmployeeSlots,
    bool IsActive,
    DateTime CreatedAt);

public record OrganizationBrandingDto(
    string Name,
    string? LogoUrl,
    string? PrimaryColor,
    string? SecondaryColor);

public record CreateOrganizationRequest(
    string Name,
    string Slug,
    string Email,
    string? PhoneNumber,
    string? Address,
    int MaxEmployeeSlots);

public record UpdateOrganizationRequest(
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? PrimaryColor,
    string? SecondaryColor,
    int MaxEmployeeSlots);
