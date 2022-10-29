namespace DoctorsOffice.SendGrid.Config;

public class SendGridSettings
{
    public string ApiKey { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
}