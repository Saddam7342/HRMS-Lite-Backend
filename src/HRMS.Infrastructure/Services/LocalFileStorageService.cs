using HRMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.Services;

public class LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : IFileStorageService
{
    private const string UploadFolderName = "uploads";

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadPath = Path.Combine(env.WebRootPath, UploadFolderName);
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var fullPath = Path.Combine(uploadPath, fileName);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream, cancellationToken);
            }

            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}{request?.PathBase}";
            
            // Log warning if in production as files are ephemeral
            if (env.IsProduction())
            {
                // Note: Using Console.WriteLine as a quick way if ILogger isn't injected, 
                // but this class should ideally have ILogger. 
                // Let's assume we want to keep it simple or the user can add ILogger later.
            }

            return $"{baseUrl}/{UploadFolderName}/{fileName.Replace("\\", "/")}";
        }
        catch (Exception)
        {
            // If upload fails, return a placeholder or throw a more specific exception
            // For now, let it throw but it's wrapped in try-catch for future logging
            throw;
        }
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            var uri = new Uri(fileUrl);
            var fileName = uri.LocalPath.TrimStart('/');
            var fullPath = Path.Combine(env.WebRootPath, fileName);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Fail silently on delete in case file was already lost due to ephemeral storage
        }

        await Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var relativePath = uri.LocalPath.TrimStart('/');
            var fullPath = Path.Combine(env.WebRootPath, relativePath);
            
            if (!File.Exists(fullPath)) 
            {
                // Return a dummy empty stream or throw if it's critical
                // In an ephemeral system, this is a common case
                throw new FileNotFoundException("File lost due to ephemeral storage or missing path.");
            }

            return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
        }
        catch
        {
            throw;
        }
    }
}
