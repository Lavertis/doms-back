using System.Security.Claims;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AuthServiceTests
{
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;
    private readonly IJwtService _fakeJwtService;
    private readonly IOptions<AppSettings> _fakeAppSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeJwtService = A.Fake<IJwtService>();
        _fakeAppSettings = A.Fake<IOptions<AppSettings>>();
        _authService = new AuthService(_fakeUserManager, _fakeJwtService, _fakeUserService, _fakeAppSettings);
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

        var loginRequest = new AuthenticateRequest { UserName = "dummyUserName", Password = "dummyPassword" };
        const string ipAddress = "dummyIpAddress";
        var cancellationToken = new CancellationToken();

        // act
        var result = await _authService.AuthenticateAsync(loginRequest, ipAddress, cancellationToken);

        // assert 
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.JwtToken.Should().Be(dummyJwtToken);
        result.RefreshToken.Should().Be(dummyRefreshToken.Token);
    }

    [Fact]
    public async void RefreshToken_ValidToken_ReturnsAuthenticateResponseWithNewToken()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var oldRefreshToken = new RefreshToken
            { Token = "oldRefreshToken", Revoked = null, Expires = DateTime.UtcNow.Add(5.Minutes()) };
        var newRefreshToken = new RefreshToken { Token = "newRefreshToken" };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { oldRefreshToken } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserService.GetUserRolesAsClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() => _fakeJwtService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(newRefreshToken);
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var result = await _authService.RefreshTokenAsync(oldRefreshToken.Token, ipAddress);

        // assert
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.JwtToken.Should().Be(dummyJwtToken);
        result.RefreshToken.Should().Be(newRefreshToken.Token);
    }

    [Fact]
    public async void RefreshToken_NonExistingToken_ThrowsException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
            { Token = "oldRefreshToken", Revoked = null, Expires = DateTime.UtcNow.Add(5.Minutes()) };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { oldRefreshToken } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";
        const string notExistingToken = "notExistingToken";

        // act
        var action = async () => await _authService.RefreshTokenAsync(notExistingToken, ipAddress);

        // assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async void RefreshToken_RevokedToken_ThrowsBadRequestException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            Revoked = DateTime.UtcNow.Subtract(5.Minutes()),
            Expires = DateTime.UtcNow.Add(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { oldRefreshToken } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var action = async () => await _authService.RefreshTokenAsync(oldRefreshToken.Token, ipAddress);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RefreshToken_RevokedToken_RevokesDescendantTokens()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Id = 10,
            Token = "oldRefreshToken",
            Revoked = DateTime.UtcNow.Subtract(5.Minutes()),
            Expires = DateTime.UtcNow.Add(5.Minutes()),
            ReplacedByToken = "descendantRefreshToken1"
        };
        var descendantRefreshToken1 = new RefreshToken
        {
            Id = 11,
            Token = "descendantRefreshToken1",
            Revoked = DateTime.UtcNow.Subtract(5.Minutes()),
            Expires = DateTime.UtcNow.Add(5.Minutes()),
            ReplacedByToken = "descendantRefreshToken2"
        };
        var descendantRefreshToken2 = new RefreshToken
        {
            Id = 11,
            Token = "descendantRefreshToken2",
            Revoked = null,
            Expires = DateTime.UtcNow.Add(5.Minutes())
        };

        var fakeAppUserQueryable = new List<AppUser>
        {
            new()
            {
                RefreshTokens = new List<RefreshToken>
                    { oldRefreshToken, descendantRefreshToken1, descendantRefreshToken2 }
            }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        try
        {
            await _authService.RefreshTokenAsync(oldRefreshToken.Token, ipAddress);
        }
        catch (Exception)
        {
            // ignore
        }

        // assert
        descendantRefreshToken2.Revoked.Should().NotBeNull();
    }

    [Fact]
    public async void RefreshToken_ExpiredToken_ThrowsBadRequestException()
    {
        // arrange
        var oldRefreshToken = new RefreshToken
        {
            Token = "oldRefreshToken",
            Revoked = null,
            Expires = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { oldRefreshToken } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var action = async () => await _authService.RefreshTokenAsync(oldRefreshToken.Token, ipAddress);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_ValidToken_ReturnsTrue()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
            { Token = "refreshTokenToBeRevoked", Revoked = null, Expires = DateTime.UtcNow.Add(5.Minutes()) };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var result = await _authService.RevokeRefreshTokenAsync(refreshTokenToBeRevoked.Token, ipAddress);

        // assert
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.Should().BeTrue();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_ValidToken_RevokesToken()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
            { Token = "refreshTokenToBeRevoked", Revoked = null, Expires = DateTime.UtcNow.Add(5.Minutes()) };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        await _authService.RevokeRefreshTokenAsync(refreshTokenToBeRevoked.Token, ipAddress);

        // assert
        refreshTokenToBeRevoked.Revoked.Should().NotBeNull();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_EmptyToken_ThrowsBadRequestException()
    {
        // arrange

        // act
        var action = async () => await _authService.RevokeRefreshTokenAsync(string.Empty, "dummyIpAddress");

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_NullToken_ThrowsBadRequestException()
    {
        // arrange

        // act
        var action = async () => await _authService.RevokeRefreshTokenAsync(null!, "dummyIpAddress");

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_NotExistingToken_ThrowsException()
    {
        // arrange
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken>() }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var action = async () => await _authService.RevokeRefreshTokenAsync("notExistingToken", ipAddress);

        // assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_RevokedToken_ThrowsBadRequestException()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            Revoked = DateTime.UtcNow.Subtract(5.Minutes()),
            Expires = DateTime.UtcNow.Add(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var action = async () => await _authService.RevokeRefreshTokenAsync(refreshTokenToBeRevoked.Token, ipAddress);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void RevokeRefreshTokenAsync_ExpiredToken_ThrowsBadRequestException()
    {
        // arrange
        var refreshTokenToBeRevoked = new RefreshToken
        {
            Token = "refreshTokenToBeRevoked",
            Revoked = null,
            Expires = DateTime.UtcNow.Subtract(5.Minutes())
        };
        var fakeAppUserQueryable = new List<AppUser>
        {
            new() { RefreshTokens = new List<RefreshToken> { refreshTokenToBeRevoked } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        const string ipAddress = "dummyIpAddress";

        // act
        var action = async () => await _authService.RevokeRefreshTokenAsync(refreshTokenToBeRevoked.Token, ipAddress);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }
}