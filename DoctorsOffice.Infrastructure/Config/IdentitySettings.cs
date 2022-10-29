namespace DoctorsOffice.Infrastructure.Config;

public class IdentitySettings
{
    public int EmailConfirmationTokenLifeSpanInHours { get; set; }
    public int PasswordResetTokenLifeSpanInHours { get; set; }
}