using System.Security.Claims;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class JwtServiceTests
{
    private readonly JwtSettings _fakeJwtSettings;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        _fakeJwtSettings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012",
            TokenLifetimeInMinutes = 15,
            RefreshTokenLifetimeInDays = 7,
            RefreshTokenTtlInDays = 3
        };
        var fakeJwtSettingsOptions = A.Fake<IOptions<JwtSettings>>();
        A.CallTo(() => fakeJwtSettingsOptions.Value).Returns(_fakeJwtSettings);
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _jwtService = new JwtService(fakeJwtSettingsOptions, _fakeUserManager);
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
    public async Task GenerateRefreshToken_ReturnsRefreshToken()
    {
        // arrange
        const string ipAddress = "dummyIpAddress";
        var cancellationToken = A.Dummy<CancellationToken>();
        var dummyUsersQueryable = A.CollectionOfDummy<AppUser>(1).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(dummyUsersQueryable);

        // act
        var result = await _jwtService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);

        // assert
        result.Should().NotBeNull();
    }
}