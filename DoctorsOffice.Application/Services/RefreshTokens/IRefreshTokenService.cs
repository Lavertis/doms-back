using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.Services.RefreshTokens;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string? ipAddress, CancellationToken cancellationToken = default);
    public void RemoveOldRefreshTokens(AppUser user);

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null,
        string? replacedByToken = null);
}