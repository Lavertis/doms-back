using System.Security.Claims;
using DoctorsOffice.Application.CQRS.Commands.Authenticate;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AuthHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IJwtService _fakeJwtService;
    private readonly IRefreshTokenService _fakeRefreshTokenService;

    public AuthHandlerTests()
    {
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _fakeJwtService = A.Fake<IJwtService>();
        _fakeRefreshTokenService = A.Fake<IRefreshTokenService>();
    }

    [Fact]
    public async void AuthenticateHandler_ValidData_UpdatesUserAndReturnsAuthResponse()
    {
        // arrange
        const string dummyJwtToken = "dummyJwtToken";
        var dummyRefreshToken = new RefreshToken {Token = "dummyRefreshToken"};

        A.CallTo(() => _fakeAppUserManager.FindByNameAsync(A<string>.Ignored))
            .Returns(new CommonResult<AppUser>().WithValue(new AppUser()));
        A.CallTo(() => _fakeAppUserManager.ValidatePasswordAsync(A<string>.Ignored, A<string>.Ignored))
            .Returns(new CommonResult<bool>().WithValue(true));
        A.CallTo(() => _fakeJwtService.GetUserClaimsAsync(A<AppUser>.Ignored)).Returns(new List<Claim>());
        A.CallTo(() => _fakeJwtService.GenerateJwtToken(A<IList<Claim>>.Ignored)).Returns(dummyJwtToken);
        A.CallTo(() =>
                _fakeRefreshTokenService.GenerateRefreshTokenAsync(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dummyRefreshToken);

        const string ipAddress = "dummyIpAddress";

        var request = new AuthenticateRequest
        {
            UserName = "dummyUserName",
            Password = "dummyPassword"
        };
        var command = new AuthenticateCommand(request, ipAddress);
        var handler = new AuthenticateHandler(_fakeJwtService, _fakeRefreshTokenService, _fakeAppUserManager);

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert 
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappened();
        result.Value!.JwtToken.Should().Be(dummyJwtToken);
        result.Value.RefreshToken.Should().Be(dummyRefreshToken.Token);
    }
}