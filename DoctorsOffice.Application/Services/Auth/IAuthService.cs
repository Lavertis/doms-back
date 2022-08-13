using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.Services.Auth;

public interface IAuthService
{
    public Task<AppUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    public void RemoveOldRefreshTokens(AppUser user);

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null,
        string? replacedByToken = null);
}