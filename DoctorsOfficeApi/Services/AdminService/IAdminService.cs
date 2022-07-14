using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Services.AdminService;

public interface IAdminService
{
    Task<Admin> GetAdminByIdAsync(string id);
}