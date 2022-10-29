namespace DoctorsOffice.Domain.DTO.Responses;

public class CreateDoctorResponse
{
    public DoctorResponse Doctor { get; set; } = null!;
    public string PasswordResetLink { get; set; } = null!;
    public string PasswordResetToken { get; set; } = null!;
}