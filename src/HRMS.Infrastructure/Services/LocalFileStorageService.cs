using HRMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.Services;

public class LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : IFileStorageService
{
    private const string UploadFolderName = "uploads";

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadPath = Path.Combine(env.WebRootPath, UploadFolderName);
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        // Subfolder logic (optional but recommended)
        // For this module, we use the filename to determine path or pass it in.
        // Simplified: store in uploads/ directly for now or preserve folder structure if fileName contains it.
        
        var fullPath = Path.Combine(uploadPath, fileName);
        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, cancellationToken);
        }

        var request = httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host}{request?.PathBase}";
        return $"{baseUrl}/{UploadFolderName}/{fileName.Replace("\\", "/")}";
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var uri = new Uri(fileUrl);
        var fileName = uri.LocalPath.TrimStart('/');
        // Remove 'uploads/' from path to get relative path inside wwwroot
        var relativePath = fileName;
        
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        await Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(fileUrl);
        var relativePath = uri.LocalPath.TrimStart('/');
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        
        if (!File.Exists(fullPath)) throw new FileNotFoundException();

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }
}
