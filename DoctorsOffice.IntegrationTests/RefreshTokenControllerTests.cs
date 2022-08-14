using System.Net;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class RefreshTokenControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/refresh-tokens";

    public RefreshTokenControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRefreshTokensByUserId_UserDoesntHaveRefreshTokens_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var user = new AppUser
        {
            UserName = "user1",
            Id = Guid.NewGuid()
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/{user.Id}");

        // assert
        var responseContent = await response.Content.ReadAsAsync<List<RefreshToken>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().BeEmpty();
    }

    [Theory]
    [InlineData(RoleTypes.Doctor)]
    [InlineData(RoleTypes.Patient)]
    public async Task GetRefreshTokensByUserId_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var dummyUserId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/{dummyUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRefreshTokensByUserId_RequestedUserExists_ReturnsAllRefreshTokens()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var user = new AppUser
        {
            UserName = "user1",
            Id = Guid.NewGuid()
        };
        var refreshTokens = new List<RefreshToken>
        {
            new() {Id = Guid.NewGuid(), Token = "token1"},
            new() {Id = Guid.NewGuid(), Token = "token2"},
            new() {Id = Guid.NewGuid(), Token = "token3"},
        };
        user.RefreshTokens.AddRange(refreshTokens);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/{user.Id}");

        // assert
        var responseContent = await response.Content.ReadAsAsync<List<RefreshToken>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().BeEquivalentTo(refreshTokens);
    }

    [Fact]
    public async Task GetRefreshTokensByUserId_RequestedUserDoesNotExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string nonExistingUserId = "nonExistingUserId";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/{nonExistingUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeToken_TokenExists_RevokesToken()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string testToken = "testToken";
        var testUser = new AppUser
        {
            UserName = "testUser",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke", request);

        // assert
        RefreshDbContext();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await DbContext.Users.FindAsync(testUser.Id))!
            .RefreshTokens.SingleOrDefault(token => token.Token == testToken)!
            .RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_TokenDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var request = new RevokeRefreshTokenRequest {RefreshToken = "nonExistingToken"};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeToken_TokenIsExpired_ReturnsBadRequestException()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string testToken = "testToken";
        var testUser = new AppUser {UserName = "testUser"};
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.Subtract(1.Days())
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevokeToken_TokenIsRevoked_ReturnsBadRequestException()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string testToken = "testToken";
        var testUser = new AppUser {UserName = "testUser"};
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            RevokedAt = DateTime.UtcNow.Subtract(1.Days()),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}