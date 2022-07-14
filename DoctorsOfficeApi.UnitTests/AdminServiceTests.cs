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
        var inMemoryDbName = "InMemoryDb_" + DateTime.Now.ToFileTimeUtc();
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
            AppUser = new AppUser { Id = "100" }
        };

        _dbContext.Admins.Add(admin);
        await _dbContext.SaveChangesAsync();

        // act
        var result = await _adminService.GetAdminByIdAsync("100");

        // assert
        result.Should().BeEquivalentTo(admin);
    }

    [Fact]
    public async Task GetAdminById_AdminDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string adminId = "100";

        // act
        var action = async () => await _adminService.GetAdminByIdAsync(adminId);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}