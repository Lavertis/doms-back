using DoctorsOfficeApi.CQRS.Queries.GetAllUsers;
using DoctorsOfficeApi.CQRS.Queries.GetRefreshTokensByUserId;
using DoctorsOfficeApi.CQRS.Queries.GetUserById;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class UserHandlerTests
{
    private readonly RoleManager<AppRole> _fakeRoleManager;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;

    public UserHandlerTests()
    {
        _fakeRoleManager = A.Fake<RoleManager<AppRole>>();
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeUserService = A.Fake<IUserService>();
    }

    [Fact]
    public async void GetAllUsersHandler_NoUsers_ReturnsEmptyCollection()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async void GetAllUsersHandler_UsersExist_ReturnsAllUsers()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(3).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async void GetAllUsersHandler_OneUserExists_ReturnsCollectionWithOneUser()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(1).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async void GetUserByIdHandler_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(_fakeUserService);

        // act
        var action = async () => await handler.Handle(query, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async void GetUserByIdHandler_IdExists_ReturnsUser()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Returns(appUser);

        var expectedResponse = new UserResponse(appUser);

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(_fakeUserService);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdExists_ReturnsRefreshTokens()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        var refreshTokens = new List<RefreshToken> {A.Dummy<RefreshToken>()};
        A.CallTo(() => appUser.RefreshTokens).Returns(refreshTokens);
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Returns(appUser);

        var query = new GetRefreshTokensByUserIdQuery(Guid.NewGuid());
        var handler = new GetRefreshTokensByUserIdHandler(_fakeUserService);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(refreshTokens);
    }

    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));

        var query = new GetRefreshTokensByUserIdQuery(Guid.NewGuid());
        var handler = new GetRefreshTokensByUserIdHandler(_fakeUserService);

        // act
        var action = async () => await handler.Handle(query, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}