using HospitalMS.Common.Storage;
using Microsoft.Extensions.Configuration;

namespace HospitalMS.Business.Services;

public sealed class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private string BasePath => configuration["FileStorage:BasePath"]
        ?? Path.Combine(Path.GetTempPath(), "hospitalms-files");

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(BasePath);

        var ext = Path.GetExtension(fileName);
        var storageName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(BasePath, storageName);

        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);

        return storageName;
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(BasePath, storagePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored file not found.", storagePath);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(BasePath, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
