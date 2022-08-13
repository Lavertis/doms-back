using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.Services.User;

public interface IUserService
{
    Task<AppUser> GetUserByIdAsync(Guid userId);
    Task<AppUser> GetUserByUserNameAsync(string userName);
    Task<AppUser> CreateUserAsync(CreateUserRequest request);
    Task<AppUser> UpdateUserByIdAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserByIdAsync(Guid userId);
    Task<IList<Claim>> GetUserRolesAsClaimsAsync(AppUser user);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ValidateUserPasswordAsync(string userName, string password);
    void SetUserPassword(AppUser user, string newPassword);
    Task<bool> UserExistsByIdAsync(Guid userId);
}