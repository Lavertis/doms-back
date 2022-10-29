namespace DoctorsOffice.Domain.DTO.Requests;

public class PasswordSetRequest
{
    public string NewPassword { get; set; } = null!;
    public string PasswordResetToken { get; set; } = null!;
    public string Email { get; set; } = null!;
}