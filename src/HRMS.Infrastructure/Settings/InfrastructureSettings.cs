namespace HRMS.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}

public class MailSettings
{
    public const string SectionName = "MailSettings";
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class FileStorageSettings
{
    public const string SectionName = "FileStorageSettings";
    public string UploadPath { get; set; } = "uploads";
    public string Provider { get; set; } = "Local"; // Local, Azure, S3
}
