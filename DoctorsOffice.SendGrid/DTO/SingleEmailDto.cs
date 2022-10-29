namespace DoctorsOffice.SendGrid.DTO;

public class SingleEmailDto
{
    public string RecipientEmail { get; set; } = null!;
    public string RecipientName { get; set; } = null!;
    public string TemplateId { get; set; } = null!;
    public object? TemplateData { get; set; }
}