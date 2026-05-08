namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// File storage abstraction (local, Azure Blob, S3, etc.).
/// Application layer never touches storage SDK directly.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);
}
