using AutoMapper;
using DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class UserHandlerTests
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IMapper _fakeMapper;

    public UserHandlerTests()
    {
        _fakeMapper = A.Fake<IMapper>();
        _fakeAppUserManager = A.Fake<AppUserManager>();
    }

    [Fact]
    public async void GetAllUsersHandler_NoUsers_ReturnsEmptyCollection()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, _fakeMapper);

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
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, _fakeMapper);

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
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, _fakeMapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public async void GetUserByIdHandler_IdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithError(new Error()));

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(_fakeMapper, _fakeAppUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async void GetUserByIdHandler_IdExists_ReturnsUser()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithValue(appUser));

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
        var handler = new GetUserByIdHandler(_fakeMapper, _fakeAppUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }
}