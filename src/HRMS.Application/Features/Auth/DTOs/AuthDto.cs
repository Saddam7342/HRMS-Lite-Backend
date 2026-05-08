namespace HRMS.Application.Features.Auth.DTOs;

public record LoginRequest(string EmailOrUsername, string Password, Guid OrganizationId);

public record TokenDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record LoginResponse(
    Guid UserId,
    string Email,
    string FullName,
    TokenDto Token,
    List<string> Roles,
    List<string> Permissions);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    Guid OrganizationId,
    List<string> Roles,
    List<string> Permissions);

public record RoleDto(Guid Id, string Name, string? Description);

public record PermissionDto(Guid Id, string Name, string Code, string Module);
