using Droppy.Backend.DTOs;

namespace Droppy.Backend.Interfaces;

public  interface  IFileStorageService
{
    
    Task<string>UploadAsync(IFormFile file,
        
        string userScope,
        CancellationToken cancellationToken=default);
    
    Task<IReadOnlyList<FileObjectDto>> ListAsync(
        string userScope,
        CancellationToken cancellationToken = default);
    Task<Stream>DownloadAsync(string key,CancellationToken cancellationToken=default);
    
    
    Task<string> GenerateDownloadUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}