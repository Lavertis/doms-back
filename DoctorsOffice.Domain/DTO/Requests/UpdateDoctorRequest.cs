namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateDoctorRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
}