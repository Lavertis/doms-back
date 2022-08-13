namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateUserRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string RoleName { get; set; } = default!;
}