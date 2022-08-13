namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateDoctorRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
}