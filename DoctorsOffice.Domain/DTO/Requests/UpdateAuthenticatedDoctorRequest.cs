namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateAuthenticatedDoctorRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
    public string CurrentPassword { get; set; } = null!;
}