using DoctorsOfficeApi.Config;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.AuthService;
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
    private readonly AuthService _authService;
    private readonly IOptions<AppSettings> _fakeAppSettings;
    private readonly UserManager<AppUser> _fakeUserManager;

    public AuthServiceTests()
    {
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeAppSettings = A.Fake<IOptions<AppSettings>>();
        _authService = new AuthService(_fakeUserManager, _fakeAppSettings);
    }

    [Fact]
    public async Task GetUserByRefreshToken_UserExists_ReturnsUserWhoOwnsSpecifiedToken()
    {
        // arrange
        const string refreshToken = "refreshToken";
        var usersQueryable = new List<AppUser>
        {
            new()
            {
                RefreshTokens = new List<RefreshToken>
                {
                    new() {Token = refreshToken, ExpiresAt = DateTime.Now.AddDays(1)}
                }
            }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

        // act
        var result = await _authService.GetUserByRefreshTokenAsync(refreshToken);

        // assert
        result.Should().Be(usersQueryable.First());
    }

    [Fact]
    public async Task GetUserByRefreshToken_UserDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string refreshToken = "refreshToken";
        var usersQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

        // act
        var action = async () => await _authService.GetUserByRefreshTokenAsync(refreshToken);

        // assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void RemoveOldRefreshTokens_UserHasRefreshTokens_RemovesOldTokens()
    {
        // arrange
        const string refreshToken = "refreshToken";
        var user = new AppUser
        {
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.AddDays(1)
                },
                new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.AddDays(1),
                    RevokedAt = DateTime.Now.Subtract(1.Days())
                },
                new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.Subtract(1.Days())
                }
            }
        };

        // act
        _authService.RemoveOldRefreshTokens(user);

        // assert
        user.RefreshTokens.Should().NotContain(token => !token.IsActive);
    }

    [Fact]
    public void RevokeRefreshToken_NotRevokedToken_RevokesToken()
    {
        // arrange
        const string refreshToken = "refreshToken";
        var token = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        const string reasonRevoked = "reason";
        const string ipAddress = "ipAddress";
        const string replacedByToken = "replacedByToken";

        // act
        _authService.RevokeRefreshToken(token, ipAddress, reasonRevoked, replacedByToken);

        // assert
        token.RevokedAt.Should().NotBeNull();
        token.ReasonRevoked.Should().Be(reasonRevoked);
        token.RevokedByIp.Should().Be(ipAddress);
        token.ReplacedByToken.Should().Be(replacedByToken);
    }
}