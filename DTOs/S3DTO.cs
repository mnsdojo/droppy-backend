namespace Droppy.Backend.DTOs;


public class FileObjectDto
{
    public string Key { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public long Size { get; init; }
    public DateTime LastModified { get; init; }
}