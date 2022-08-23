using DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class RefreshTokenHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;

    public RefreshTokenHandlerTests()
    {
        _fakeAppUserManager = A.Fake<AppUserManager>();
    }


    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdExists_ReturnsRefreshTokens()
    {
        // arrange
        var userId = Guid.NewGuid();
        var refreshTokens = new List<RefreshToken> {A.Dummy<RefreshToken>()};
        var usersQueryable = new List<AppUser> {new() {Id = userId, RefreshTokens = refreshTokens}}.AsQueryable()
            .BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);

        var query = new GetRefreshTokensByUserIdQuery(userId);
        var handler = new GetRefreshTokensByUserIdHandler(_fakeAppUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(refreshTokens);
    }

    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var usersQueryable = new List<AppUser> {new() {Id = Guid.NewGuid()}}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);

        var query = new GetRefreshTokensByUserIdQuery(Guid.NewGuid());
        var handler = new GetRefreshTokensByUserIdHandler(_fakeAppUserManager);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}