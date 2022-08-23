namespace DoctorsOffice.Domain.DTO.Requests;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}