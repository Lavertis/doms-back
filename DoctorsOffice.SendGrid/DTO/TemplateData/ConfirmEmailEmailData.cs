namespace DoctorsOffice.SendGrid.DTO.TemplateData;

public class ConfirmEmailTemplateData
{
    public int ExpirationTimeInHours { get; set; }
    public string ConfirmationLink { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string WebsiteAddress { get; set; } = null!;
}