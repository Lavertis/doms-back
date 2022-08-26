using System.Security.Claims;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class JwtServiceTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        var fakeJwtSettings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012",
            TokenLifetimeInMinutes = 15,
            RefreshTokenLifetimeInDays = 7,
            RefreshTokenTtlInDays = 3
        };
        var fakeJwtSettingsOptions = A.Fake<IOptions<JwtSettings>>();
        A.CallTo(() => fakeJwtSettingsOptions.Value).Returns(fakeJwtSettings);
        A.Fake<AppRoleManager>();
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _jwtService = new JwtService(fakeJwtSettingsOptions, _fakeAppUserManager);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsJwtToken()
    {
        // Arrange
        var claims = A.CollectionOfDummy<Claim>(3);

        // Act
        var result = _jwtService.GenerateJwtToken(claims);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async void GetUserClaimsAsync_UserDoesntHaveRoles_DoesntReturnAnyRoleClaim()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeAppUserManager.GetRolesAsync(A<AppUser>.Ignored)).Returns(new List<string>());

        // act
        var result = await _jwtService.GetUserClaimsAsync(appUser);

        // assert
        result.Should().NotContain(claim => claim.Type == ClaimTypes.Role);
    }

    [Fact]
    public async void GetUserClaimsAsync_UserHasRoles_ReturnsRoleClaims()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        var roles = new List<string> {"role1", "role2"};
        A.CallTo(() => _fakeAppUserManager.GetRolesAsync(A<AppUser>.Ignored)).Returns(roles);

        // act
        var result = await _jwtService.GetUserClaimsAsync(appUser);

        // assert
        result.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "role1");
        result.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "role2");
    }
}