namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateAuthenticatedPatientRequest
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NewPassword { get; set; }
    public string CurrentPassword { get; set; } = null!;
}