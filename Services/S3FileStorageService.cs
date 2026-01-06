using Droppy.Backend.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Droppy.Backend.DTOs;
using Microsoft.Extensions.Options;

namespace Droppy.Backend.Services;

public class S3FileStorageService:IFileStorageService
{
 private readonly IAmazonS3 _s3;
 private readonly string _bucketName;
 private readonly FileUploadOptions _fileUploadOptions;

 public S3FileStorageService(IAmazonS3 s3, IConfiguration configuration,
        IOptions<FileUploadOptions> fileUploadOptions
     )
 {
  _s3 = s3;
  _fileUploadOptions = fileUploadOptions.Value;
    
  _bucketName = configuration["AWS:S3:BucketName"]
                ?? throw new InvalidOperationException("AWS S3 bucket name is required");
 }

 public async Task<string> UploadAsync(IFormFile file,
     
     string userScope,
     CancellationToken cancellationToken = default)
 {
     if (file == null || file.Length == 0)
     {
         throw new ArgumentException("File is required");
     }

     var maxBytes = _fileUploadOptions.MaxFileSizeMB * 1024L * 1024L;
     if (file.Length > maxBytes)
     {
         throw new InvalidOperationException("File size exceeds limit");
         
     }

     var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
     if (!_fileUploadOptions.AllowedExtensions.Contains(extension))
     {
         throw new InvalidOperationException("File type not allowed!");
         
     }

     var key = $"users/{userScope}/{Guid.NewGuid()}/{file.FileName}";
        await   using var stream = file.OpenReadStream();
        var uploadRequest = new TransferUtilityUploadRequest
        {
            
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };
        var transferUtility = new TransferUtility(_s3);
        await transferUtility.UploadAsync(uploadRequest, cancellationToken);
        return key;
 }

 public async Task<IReadOnlyList<FileObjectDto>> ListAsync(string userScope, CancellationToken cancellationToken = default)
 {
     var prefix = $"users/{userScope}/";
     var request = new ListObjectsV2Request
     {
         BucketName = _bucketName,
         Prefix = prefix
     };
     
     
     var response = await _s3.ListObjectsV2Async(request, cancellationToken);
     var result = response.S3Objects
         .Where(o => !o.Key.EndsWith("/"))
         .Select(o => new FileObjectDto
         {
             Key = o.Key,
             FileName = Path.GetFileName(o.Key),
             Size = o.Size??0,
             LastModified = o.LastModified.HasValue ? o.LastModified.Value.ToUniversalTime() : DateTime.MinValue,

         }).ToList();
     return result;

 }

 public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
 {
    var response = await  _s3.GetObjectAsync(_bucketName, key, cancellationToken);
    return response.ResponseStream;
 }

 public  Task<string> GenerateDownloadUrlAsync(string key, TimeSpan expiry,
     CancellationToken cancellationToken = default)
 {
     var request = new GetPreSignedUrlRequest
     {
         BucketName = _bucketName,
         Key = key,
         Expires = DateTime.UtcNow.Add(expiry)
     };
     return Task.FromResult(_s3.GetPreSignedURL(request));
 
 }
 
 
}