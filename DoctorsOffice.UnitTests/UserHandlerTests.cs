using DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class UserHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;

    public UserHandlerTests()
    {
        _fakeAppUserManager = A.Fake<AppUserManager>();
    }

    [Fact]
    public async void GetAllUsersHandler_NoUsers_ReturnsEmptyCollection()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async void GetAllUsersHandler_UsersExist_ReturnsAllUsers()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(3).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().HaveCount(3);
    }

    [Fact]
    public async void GetAllUsersHandler_OneUserExists_ReturnsCollectionWithOneUser()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(1).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().ContainSingle();
    }

    [Fact]
    public async void GetAllUsersHandler_NoPaginationProvided_ReturnsAllUsers()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(3).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var expectedResponse = await fakeAppUserQueryable
            .Select(u => Mapper.Map<UserResponse>(u))
            .ToListAsync();

        var query = new GetAllUsersQuery();
        var handler = new GetAllUsersHandler(_fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }


    [Fact]
    public async void GetAllUsersHandler_PaginationProvided_ReturnsAllUsers()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(20).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        const int pageSize = 5;
        const int pageNumber = 3;

        var expectedResponse = await fakeAppUserQueryable
            .Select(u => Mapper.Map<UserResponse>(u))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var query = new GetAllUsersQuery
        {
            PaginationFilter = new PaginationFilter {PageNumber = pageNumber, PageSize = pageSize}
        };
        var handler = new GetAllUsersHandler(_fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async void GetUserByIdHandler_IdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithError(new Error()));

        var query = new GetUserByIdQuery(Guid.NewGuid());
        var handler = new GetUserByIdHandler(Mapper, _fakeAppUserManager);

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
        var handler = new GetUserByIdHandler(Mapper, _fakeAppUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }
}