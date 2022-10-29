namespace DoctorsOffice.SendGrid.DTO.TemplateData;

public class PasswordResetEmailData
{
    public string FirstName { get; set; } = null!;
    public string PasswordResetLink { get; set; } = null!;
    public int ExpirationTimeInHours { get; set; }
}