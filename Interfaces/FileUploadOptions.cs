namespace Droppy.Backend.Interfaces;

public class FileUploadOptions
{
    public int MaxFileSizeMB { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}