namespace HospitalMS.Common.Storage;

public interface IFileStorageService
{
    /// <summary>Uploads content and returns the storage path for later retrieval.</summary>
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
