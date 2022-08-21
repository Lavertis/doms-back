using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;

namespace DoctorsOffice.Application.Services.Users;

public interface IUserService
{
    Task<HttpResult<AppUser>> CreateUserAsync(CreateUserRequest request);
    Task<HttpResult<AppUser>> UpdateUserByIdAsync(Guid userId, UpdateUserRequest request);
}