using System.Net;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class AuthControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/auth";

    public AuthControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Authenticate_CredentialsAreCorrect_ReturnsJwtAndRefreshToken()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string testUserName = "testUser";
        const string testPassword = "testPassword";
        DbContext.Users.Add(new AppUser
        {
            UserName = testUserName,
            NormalizedUserName = testUserName.ToUpper(),
            PasswordHash = hasher.HashPassword(null!, testPassword),
            SecurityStamp = Guid.NewGuid().ToString()
        });
        await DbContext.SaveChangesAsync();

        var request = new AuthenticateRequest
        {
            UserName = testUserName,
            Password = testPassword
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/authenticate", request);

        // assert
        var responseContent = await response.Content.ReadAsAsync<AuthenticateResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.JwtToken.Should().NotBeNullOrEmpty();
        responseContent.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authenticate_UserNameDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var request = new AuthenticateRequest
        {
            UserName = "nonExistingUser",
            Password = "dummyPassword"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/authenticate", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Authenticate_PasswordIsIncorrect_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string testUserName = "testUser";
        const string testPassword = "testPassword";
        DbContext.Users.Add(new AppUser
        {
            UserName = testUserName,
            NormalizedUserName = testUserName.ToUpper(),
            PasswordHash = hasher.HashPassword(null!, testPassword)
        });
        await DbContext.SaveChangesAsync();

        var request = new AuthenticateRequest
        {
            UserName = testUserName,
            Password = "incorrectPassword"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/authenticate", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsJwtAndRefreshToken()
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

        var request = new RefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        var responseContent = await response.Content.ReadAsAsync<AuthenticateResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.JwtToken.Should().NotBeNullOrEmpty();
        responseContent.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_NonExistingToken_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var request = new RefreshTokenRequest {RefreshToken = "nonExistingToken"};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_TokenIsExpired_ReturnsBadRequest()
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

        var request = new RefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_TokenIsRevoked_ReturnsBadRequest()
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
            RevokedAt = DateTime.UtcNow.Subtract(1.Hours()),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_TokenIsRevoked_RemovesDescendantTokens()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string testToken = "testToken";
        const string descendantToken = "descendantToken";
        var testUser = new AppUser
        {
            UserName = "testUser",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        testUser.RefreshTokens.AddRange(new[]
        {
            new RefreshToken
            {
                Token = testToken,
                RevokedAt = DateTime.UtcNow.Subtract(1.Hours()),
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                ReplacedByToken = descendantToken
            },
            new RefreshToken
            {
                Token = descendantToken,
                RevokedAt = null,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            }
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest {RefreshToken = testToken};

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        RefreshDbContext();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DbContext.Users.FindAsync(testUser.Id))!
            .RefreshTokens.SingleOrDefault(token => token.Token == descendantToken)!
            .RevokedAt.Should().NotBeNull();
    }
}