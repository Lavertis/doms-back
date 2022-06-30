using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;

namespace DoctorsOfficeApi.Services.AuthService;

public interface IAuthService
{
    Task<AuthenticateResponse> AuthenticateAsync(
        AuthenticateRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    Task<AuthenticateResponse> RefreshTokenAsync(string token, string? ipAddress);
    
    Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress);
}