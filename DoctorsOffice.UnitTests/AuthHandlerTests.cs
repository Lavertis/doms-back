﻿using System.Security.Claims;
using DoctorsOffice.Application.CQRS.Commands.Authenticate;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;
using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AuthHandlerTests
{
    private readonly IJwtService _fakeJwtService;
    private readonly IRefreshTokenService _fakeRefreshTokenService;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;

    public AuthHandlerTests()
    {
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeJwtService = A.Fake<IJwtService>();
        _fakeRefreshTokenService = A.Fake<IRefreshTokenService>();
    }

    [Fact]
    public async void Authenticate_ValidData_UpdatesUserAndReturnsAuthResponse()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var dummyRefreshToken = new RefreshToken {Token = "dummyRefreshToken"};

        A.CallTo(() => _fakeUserService.GetUserByUserNameAsync(A<string>.Ignored)).Returns(new AppUser());
        A.CallTo(() => _fakeUserService.GetUserRolesAsClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() => _fakeJwtService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dummyRefreshToken);

        const string ipAddress = "dummyIpAddress";
        var cancellationToken = new CancellationToken();

        var request = new AuthenticateRequest
        {
            UserName = "dummyUserName",
            Password = "dummyPassword"
        };
        var command = new AuthenticateCommand(request, ipAddress);
        var handler = new AuthenticateHandler(_fakeUserService, _fakeJwtService, _fakeRefreshTokenService,
            _fakeUserManager);

        // act
        var result = await handler.Handle(command, cancellationToken);

        // assert 
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.Value.JwtToken.Should().Be(dummyJwtToken);
        result.Value.RefreshToken.Should().Be(dummyRefreshToken.Token);
    }

    [Fact]
    public async void RefreshTokenHandler_ValidToken_ReturnsAuthenticateResponseWithNewToken()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var oldRefreshToken = new RefreshToken
            {Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var newRefreshToken = new RefreshToken {Token = "newRefreshToken"};
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {oldRefreshToken}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        A.CallTo(() => _fakeUserService.GetUserRolesAsClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() => _fakeJwtService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(newRefreshToken);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService,
            _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.Value.JwtToken.Should().Be(dummyJwtToken);
        result.Value.RefreshToken.Should().Be(newRefreshToken.Token);
    }

    [Fact]
    public async void RefreshTokenHandler_NonExistingToken_ThrowsException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
            {Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken> {oldRefreshToken}}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RefreshTokenRequest {RefreshToken = "nonExistingRefreshToken"};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService,
            _fakeRefreshTokenService);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowAsync<Exception>();
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
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {oldRefreshToken}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService,
            _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, new CancellationToken());

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

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService,
            _fakeRefreshTokenService);

        // act
        try
        {
            await handler.Handle(command, new CancellationToken());
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
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {oldRefreshToken}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RefreshTokenRequest {RefreshToken = oldRefreshToken.Token};
        var command = new RefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService,
            _fakeRefreshTokenService);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_ValidToken_RevokesToken()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
            {Token = "refreshTokenToBeRevoked", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes())};
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        await handler.Handle(command, new CancellationToken());

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
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NullToken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var request = new RevokeRefreshTokenRequest {RefreshToken = null!};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NotExistingToken_ThrowsException()
    {
        // arrange
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() {RefreshTokens = new List<RefreshToken>()}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var request = new RevokeRefreshTokenRequest {RefreshToken = "nonExistingRefreshToken"};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowAsync<Exception>();
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
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, new CancellationToken());

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
        var appUser = new AppUser {RefreshTokens = new List<RefreshToken> {refreshTokenToBeRevoked}};

        A.CallTo(() => _fakeRefreshTokenService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var request = new RevokeRefreshTokenRequest {RefreshToken = refreshTokenToBeRevoked.Token};
        var command = new RevokeRefreshTokenCommand(request: request, ipAddress: "dummyIpAddress");
        var handler = new RevokeRefreshTokenHandler(_fakeRefreshTokenService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}