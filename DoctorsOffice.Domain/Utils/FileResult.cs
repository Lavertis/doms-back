namespace DoctorsOffice.Domain.Utils;

public class FileResult
{
    public MemoryStream Stream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}