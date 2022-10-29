namespace DoctorsOffice.Domain.DTO.Responses;

public class PasswordResetResponse
{
    public string Token { get; set; } = null!;
    public string PasswordResetLink { get; set; } = null!;
}