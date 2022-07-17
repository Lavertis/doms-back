using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;

namespace DoctorsOfficeApi.Services.AdminService;

public class AdminService : IAdminService
{
    private readonly AppDbContext _dbContext;

    public AdminService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Admin> GetAdminByIdAsync(Guid id)
    {
        var admin = await _dbContext.Admins.FindAsync(id);
        if (admin is null)
            throw new NotFoundException("Admin not found");
        return admin;
    }
}