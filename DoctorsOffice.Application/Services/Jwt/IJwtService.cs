using System.Security.Claims;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Application.Services.Jwt;

public interface IJwtService
{
    public string GenerateJwtToken(IList<Claim> claims);

    public Task<RefreshToken> GenerateRefreshTokenAsync(
        string? ipAddress,
        CancellationToken cancellationToken = default
    );
}