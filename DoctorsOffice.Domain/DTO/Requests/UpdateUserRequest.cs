namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateUserRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}