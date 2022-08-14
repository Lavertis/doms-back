using DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Exceptions;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class RefreshTokenHandlerTests
{
    private readonly IUserService _fakeUserService;

    public RefreshTokenHandlerTests()
    {
        _fakeUserService = A.Fake<IUserService>();
    }


    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdExists_ReturnsRefreshTokens()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        var refreshTokens = new List<RefreshToken> {A.Dummy<RefreshToken>()};
        A.CallTo(() => appUser.RefreshTokens).Returns(refreshTokens);
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Returns(appUser);

        var query = new GetRefreshTokensByUserIdQuery(Guid.NewGuid());
        var handler = new GetRefreshTokensByUserIdHandler(_fakeUserService);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(refreshTokens);
    }

    [Fact]
    public async Task GetUserRefreshTokensHandler_UserIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakeUserService.GetUserByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));

        var query = new GetRefreshTokensByUserIdQuery(Guid.NewGuid());
        var handler = new GetRefreshTokensByUserIdHandler(_fakeUserService);

        // act
        var action = async () => await handler.Handle(query, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}