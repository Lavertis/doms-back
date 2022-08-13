namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateDoctorRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}