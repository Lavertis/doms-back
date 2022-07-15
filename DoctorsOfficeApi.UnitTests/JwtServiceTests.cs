using System.Security.Claims;
using DoctorsOfficeApi.Config;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Services.JwtService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class JwtServiceTests
{
    private readonly AppSettings _dummyAppSettings;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        _dummyAppSettings = new AppSettings { JwtSecretKey = "12345678901234567890123456789012" };
        var fakeAppSettingsOptions = A.Fake<IOptions<AppSettings>>();
        A.CallTo(() => fakeAppSettingsOptions.Value).Returns(_dummyAppSettings);
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _jwtService = new JwtService(fakeAppSettingsOptions, _fakeUserManager);
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