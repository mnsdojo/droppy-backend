using Droppy.Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Droppy.Backend.Controllers;

[ApiController]
[Route("api/files")]
public class FileController:ControllerBase

{
    private readonly IFileStorageService _fileStorageService;
    public FileController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload")]
    [EnableRateLimiting("uploadLimiter")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var userscope = "anonymous";
        var key = await _fileStorageService.UploadAsync(file,userscope,cancellationToken);
        return Ok(new
        {
            key,
        });
    }

    [HttpGet("download")]
    [EnableRateLimiting("downloadLimiter")]
    public async Task<IActionResult> Download([FromQuery]string key, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Key is required");
        }
        
        var stream = await _fileStorageService.DownloadAsync(key, cancellationToken);
        var fileName = Path.GetFileName(key);
        return File(stream,"application/octet-stream",fileName);
    }

    [HttpGet]
    [EnableRateLimiting("listLimiter")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userScope = "anonymous";
        var list = await _fileStorageService.ListAsync(userScope, cancellationToken);
        return Ok(list);
    }

    [HttpGet("presigned")]
    public async Task<IActionResult> Presigned([FromQuery] string key, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(key)) return BadRequest("Key is Required");
        var expiry = TimeSpan.FromMinutes(5);
        var url = await _fileStorageService.GenerateDownloadUrlAsync(
            key,
            expiry,
            cancellationToken);

        return Ok(new { url });
    }
    
}