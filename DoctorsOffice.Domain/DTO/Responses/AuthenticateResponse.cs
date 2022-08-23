namespace DoctorsOffice.Domain.DTO.Responses;

public class AuthenticateResponse
{
    public string JwtToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}