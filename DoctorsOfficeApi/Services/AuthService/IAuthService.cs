using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Services.AuthService;

public interface IAuthService
{
    public Task<AppUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    public void RemoveOldRefreshTokens(AppUser user);

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null,
        string? replacedByToken = null);
}