using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Commands.Authenticate;
using DoctorsOfficeApi.CQRS.Commands.RefreshToken;
using DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AuthHandlerTests
{
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;
    private readonly IJwtService _fakeJwtService;
    private readonly IAuthService _fakeAuthService;

    public AuthHandlerTests()
    {
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeJwtService = A.Fake<IJwtService>();
        _fakeAuthService = A.Fake<IAuthService>();
    }

    [Fact]
    public async void Authenticate_ValidData_UpdatesUserAndReturnsAuthResponse()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var dummyRefreshToken = new RefreshToken { Token = "dummyRefreshToken" };

        A.CallTo(() => _fakeUserService.GetUserByUserNameAsync(A<string>.Ignored)).Returns(new AppUser());
        A.CallTo(() => _fakeUserService.GetUserRolesAsClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() => _fakeJwtService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dummyRefreshToken);

        const string ipAddress = "dummyIpAddress";
        var cancellationToken = new CancellationToken();

        var command = new AuthenticateCommand("dummyUserName", "dummyPassword", ipAddress);
        var handler = new AuthenticateHandler(_fakeUserService, _fakeJwtService, _fakeAuthService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, cancellationToken);

        // assert 
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.JwtToken.Should().Be(dummyJwtToken);
        result.RefreshToken.Should().Be(dummyRefreshToken.Token);
    }

    [Fact]
    public async void RefreshTokenHandler_ValidToken_ReturnsAuthenticateResponseWithNewToken()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var oldRefreshToken = new RefreshToken
            { Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes()) };
        var newRefreshToken = new RefreshToken { Token = "newRefreshToken" };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { oldRefreshToken } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        A.CallTo(() => _fakeUserService.GetUserRolesAsClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() => _fakeJwtService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(newRefreshToken);

        var command = new RefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = oldRefreshToken.Token
        };
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAuthService);

        // act
        var result = await handler.Handle(command, new CancellationToken());

        // assert
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.JwtToken.Should().Be(dummyJwtToken);
        result.RefreshToken.Should().Be(newRefreshToken.Token);
    }

    [Fact]
    public async void RefreshTokenHandler_NonExistingToken_ThrowsException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
            { Token = "oldRefreshToken", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes()) };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { oldRefreshToken } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var command = new RefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = "nonExistingRefreshToken"
        };
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAuthService);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async void RefreshTokenHandler_RevokedToken_ThrowsBadRequestException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes())
        };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { oldRefreshToken } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = oldRefreshToken.Token
        };
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAuthService);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
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

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = oldRefreshToken.Token
        };
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAuthService);

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
    public async void RefreshTokenHandler_ExpiredToken_ThrowsBadRequestException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { oldRefreshToken } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = oldRefreshToken.Token
        };
        var handler = new RefreshTokenHandler(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAuthService);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_ValidToken_RevokesToken()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
            { Token = "refreshTokenToBeRevoked", RevokedAt = null, ExpiresAt = DateTime.UtcNow.Add(5.Minutes()) };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = refreshTokenToBeRevoked.Token
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        await handler.Handle(command, new CancellationToken());

        // assert
        A.CallTo(() => _fakeAuthService.RevokeRefreshToken(
            A<RefreshToken>.Ignored,
            A<string?>.Ignored,
            A<string?>.Ignored,
            A<string?>.Ignored)
        ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_EmptyToken_ThrowsBadRequestException()
    {
        // arrange
        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = string.Empty
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NullToken_ThrowsBadRequestException()
    {
        // arrange

        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = null!
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_NotExistingToken_ThrowsException()
    {
        // arrange
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken>() }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = "nonExistingRefreshToken"
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_RevokedToken_ThrowsBadRequestException()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            RevokedAt = DateTime.UtcNow.Subtract(5.Minutes()),
            ExpiresAt = DateTime.UtcNow.Add(5.Minutes())
        };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = refreshTokenToBeRevoked.Token
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenHandler_ExpiredToken_ThrowsBadRequestException()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var appUser = new AppUser { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } };

        A.CallTo(() => _fakeAuthService.GetUserByRefreshTokenAsync(
            A<string>.Ignored,
            A<CancellationToken>.Ignored)
        ).Returns(appUser);

        var command = new RevokeRefreshTokenCommand
        {
            IpAddress = "dummyIpAddress",
            RefreshToken = refreshTokenToBeRevoked.Token
        };
        var handler = new RevokeRefreshTokenHandler(_fakeAuthService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(command, new CancellationToken());

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }
}