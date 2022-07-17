using DoctorsOfficeApi.CQRS.Queries.GetAdminById;
using DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AdminService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AdminHandlerTests
{
    private readonly AppDbContext _appDbContext;
    private readonly IAdminService _fakeAdminService;

    public AdminHandlerTests()
    {
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _appDbContext = new AppDbContext(dbContextOptions);
        _fakeAdminService = A.Fake<IAdminService>();
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminExists_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var admin = new Admin
        {
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };
        A.CallTo(() => _fakeAdminService.GetAdminByIdAsync(adminId)).Returns(admin);

        var expectedResponse = new AdminResponse(admin);

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminService);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var adminId = Guid.NewGuid();
        A.CallTo(() => _fakeAdminService.GetAdminByIdAsync(adminId))
            .Throws(new NotFoundException(""));

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminService);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreAdmins_ReturnsAllAdmins()
    {
        // arrange
        var admins = new List<Admin>
        {
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } },
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } },
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } }
        };

        _appDbContext.Admins.AddRange(admins);
        await _appDbContext.SaveChangesAsync();

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var admin in admins)
            result.Should().ContainEquivalentOf(new AdminResponse(admin));
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreNoAdmins_ReturnsEmptyList()
    {
        // arrange
        _appDbContext.Admins.RemoveRange(_appDbContext.Admins);
        await _appDbContext.SaveChangesAsync();

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }
}