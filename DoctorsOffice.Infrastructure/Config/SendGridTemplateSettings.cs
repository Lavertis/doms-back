namespace DoctorsOffice.Infrastructure.Config;

public class SendGridTemplateSettings
{
    public string EmailConfirmation { get; set; } = null!;
    public string PasswordReset { get; set; } = null!;
    public string DoctorPasswordSetup { get; set; } = null!;
    public string AppointmentStatusChange { get; set; } = null!;
}