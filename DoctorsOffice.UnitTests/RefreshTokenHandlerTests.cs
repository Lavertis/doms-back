using System.Security.Claims;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;
using DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class RefreshTokenHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IJwtService _fakeJwtService;
    private readonly IRefreshTokenService _fakeRefreshTokenService;

    public RefreshTokenHandlerTests()
    {
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _fakeRefreshTokenService = A.Fake<IRefreshTokenService>();
        _fakeJwtService = A.Fake<IJwtService>();
    }


    [Fact]
    public async Task GetRefreshTokensByUserIdHandler_UserIdExists_ReturnsRefreshTokens()
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
    public async Task GetRefreshTokensByUserIdHandler_UserIdDoesntExist_ReturnsNotFound404StatusCode()
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

    [Fact]
    public async void RefreshTokenHandler_ValidToken_ReturnsAuthenticateResponseWithNewToken()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var oldRefreshToken = new RefreshToken
            {Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var newRefreshToken = new RefreshToken {Token = "newRefreshToken"};
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {oldRefreshToken}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        A.CallTo(() => _fakeJwtService.GetUserClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() =>
                _fakeRefreshTokenService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(newRefreshToken);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeAppUserManager, _fakeJwtService, _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.Value!.JwtToken.Should().Be(dummyJwtToken);
        result.Value.RefreshToken.Should().Be(newRefreshToken.Token);
    }

    [Fact]
    public async void RefreshTokenHandler_NonExistingToken_ReturnsNotFound404StatusCode()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
            {Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {oldRefreshToken}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RefreshTokenRequest {RefreshToken = "nonExistingRefreshToken"};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeAppUserManager, _fakeJwtService, _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async void RefreshTokenHandler_RevokedToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {oldRefreshToken}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeAppUserManager, _fakeJwtService, _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RefreshTokenHandler_RevokedToken_RevokesDescendantTokens()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "oldRefreshToken",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes()),
            ReplacedByToken = "descendantRefreshToken1"
        };
        var descendantRefreshToken1 = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "descendantRefreshToken1",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes()),
            ReplacedByToken = "descendantRefreshToken2"
        };
        var descendantRefreshToken2 = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "descendantRefreshToken2",
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes())
        };

        var appUser = new AppUser
        {
            RefreshTokens = new List<RefreshToken>
            {
                oldRefreshToken,
                descendantRefreshToken1,
                descendantRefreshToken2
            }
        };
        var fakeAppUserQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeAppUserManager, _fakeJwtService, _fakeRefreshTokenService);

        // act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (Exception)
        {
            // ignore
        }

        // assert
        descendantRefreshToken2.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async void RefreshTokenHandler_ExpiredToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {oldRefreshToken}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeAppUserManager, _fakeJwtService, _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_ValidToken_RevokesToken()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
            {Token = "refreshTokenToBeRevoked", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        await handler.Handle(command, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeRefreshTokenService.RevokeRefreshToken(
            A<RefreshToken>.Ignored,
            A<string?>.Ignored,
            A<string?>.Ignored,
            A<string?>.Ignored)
        ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_EmptyToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var request = new RevokeRefreshTokenRequest {RefreshToken = string.Empty};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NullToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var request = new RevokeRefreshTokenRequest {RefreshToken = null!};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NotExistingToken_ReturnsNotFound404StatusCode()
    {
        // arrange
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken>()}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RevokeRefreshTokenRequest {RefreshToken = "nonExistingRefreshToken"};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_RevokedToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_ExpiredToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}