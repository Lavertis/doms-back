namespace DoctorsOffice.Domain.DTO.Responses;

public class CreatePatientResponse
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmationLink { get; set; } = null!;
    public string EmailConfirmationToken { get; set; } = null!;
}