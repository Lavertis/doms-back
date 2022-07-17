using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.AdminService;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AdminServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly IAdminService _adminService;

    public AdminServiceTests()
    {
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        _adminService = new AdminService(_dbContext);
    }

    [Fact]
    public async Task GetAdminById_AdminExists_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var admin = new Admin
        {
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };

        _dbContext.Admins.Add(admin);
        await _dbContext.SaveChangesAsync();

        // act
        var result = await _adminService.GetAdminByIdAsync(admin.AppUser.Id);

        // assert
        result.Should().BeEquivalentTo(admin);
    }

    [Fact]
    public async Task GetAdminById_AdminDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var adminId = Guid.NewGuid();

        // act
        var action = async () => await _adminService.GetAdminByIdAsync(adminId);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}