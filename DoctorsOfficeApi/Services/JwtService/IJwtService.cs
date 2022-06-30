using System.Security.Claims;
using DoctorsOfficeApi.Entities;

namespace DoctorsOfficeApi.Services.JwtService;

public interface IJwtService
{
    public string GenerateJwtToken(IList<Claim> claims);

    public Task<RefreshToken> GenerateRefreshTokenAsync(
        string? ipAddress,
        CancellationToken cancellationToken = default
    );
}