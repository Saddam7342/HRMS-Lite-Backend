namespace HRMS.Application.Features.Settings.DTOs;

public record OrganizationSettingDto(
    Guid Id,
    string Key,
    string Value,
    string DataType,
    string? Description,
    bool IsEditable,
    DateTime? UpdatedAt);

public record UpdateSettingRequest(string Key, string Value);
public record BulkSettingsRequest(List<UpdateSettingRequest> Settings);

public record SettingsGroupDto(string GroupName, List<OrganizationSettingDto> Settings);
