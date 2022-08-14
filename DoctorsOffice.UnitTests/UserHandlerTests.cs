using AutoMapper;
using DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Exceptions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class UserHandlerTests
{
    private readonly IMapper _fakeMapper;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;

    public UserHandlerTests()
    {
        _fakeMapper = A.Fake<IMapper>();
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
        var handler = new GetAllUsersHandler(_fakeUserManager, _fakeMapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async void GetAllUsersHandler_UsersExist_ReturnsAllUsers()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(3).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeUserManager, _fakeMapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async void GetAllUsersHandler_OneUserExists_ReturnsCollectionWithOneUser()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(1).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeUserManager, _fakeMapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public async void GetUserByIdHandler_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(_fakeUserService, _fakeMapper);

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
        // appUser.UserName = appUser.NormalizedUserName = null;
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Returns(appUser);

        var expectedResponse = new UserResponse
        {
            Id = appUser.Id,
            LockoutEnabled = appUser.LockoutEnabled,
            LockoutEnd = appUser.LockoutEnd,
            UserName = appUser.UserName,
            AccessFailedCount = appUser.AccessFailedCount,
            NormalizedUserName = appUser.NormalizedUserName,
            TwoFactorEnabled = appUser.TwoFactorEnabled
        };

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(_fakeUserService, _fakeMapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }
}