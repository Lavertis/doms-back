using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class RefreshTokenServiceTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IOptions<JwtSettings> _fakeJwtSettings;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenServiceTests()
    {
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _fakeJwtSettings = A.Fake<IOptions<JwtSettings>>();
        _refreshTokenService = new RefreshTokenService(_fakeAppUserManager, _fakeJwtSettings);
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
                new()
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.AddDays(1)
                },
                new()
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.AddDays(1),
                    RevokedAt = DateTime.Now.Subtract(1.Days())
                },
                new()
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.Now.Subtract(1.Days())
                }
            }
        };

        // act
        _refreshTokenService.RemoveOldRefreshTokens(user);

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
        _refreshTokenService.RevokeRefreshToken(token, ipAddress, reasonRevoked, replacedByToken);

        // assert
        token.RevokedAt.Should().NotBeNull();
        token.ReasonRevoked.Should().Be(reasonRevoked);
        token.RevokedByIp.Should().Be(ipAddress);
        token.ReplacedByToken.Should().Be(replacedByToken);
    }

    [Fact]
    public async Task GenerateRefreshToken_ReturnsRefreshToken()
    {
        // arrange
        const string ipAddress = "dummyIpAddress";
        var cancellationToken = A.Dummy<CancellationToken>();
        var dummyUsersQueryable = A.CollectionOfDummy<AppUser>(1).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(dummyUsersQueryable);

        // act
        var result = await _refreshTokenService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);

        // assert
        result.Should().NotBeNull();
    }
}