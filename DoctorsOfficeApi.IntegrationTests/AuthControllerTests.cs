using System.Net;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

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
        var client = GetHttpClient();

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
    public async Task Authenticate_UserNameDoesntExist_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

        var request = new AuthenticateRequest
        {
            UserName = "nonExistingUser",
            Password = "dummyPassword"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/authenticate", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Authenticate_PasswordIsIncorrect_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsJwtAndRefreshToken()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken { Token = testToken, Revoked = null, Expires = DateTime.UtcNow.AddDays(1) });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = testToken };

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
        var client = GetHttpClient();

        var request = new RefreshTokenRequest { RefreshToken = "nonExistingToken" };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_TokenIsExpired_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            Revoked = null,
            Expires = DateTime.UtcNow.Subtract(1.Days())
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_TokenIsRevoked_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            Revoked = DateTime.UtcNow.Subtract(1.Hours()),
            Expires = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_TokenIsRevoked_RemovesDescendantTokens()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        const string descendantToken = "descendantToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.AddRange(new[]
        {
            new RefreshToken
            {
                Token = testToken,
                Revoked = DateTime.UtcNow.Subtract(1.Hours()),
                Expires = DateTime.UtcNow.AddDays(1),
                ReplacedByToken = descendantToken
            },
            new RefreshToken
            {
                Token = descendantToken,
                Revoked = null,
                Expires = DateTime.UtcNow.AddDays(1)
            }
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/refresh-token", request);

        // assert
        RefreshDbContext();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DbContext.Users.FindAsync(testUser.Id))!
            .RefreshTokens.SingleOrDefault(token => token.Token == descendantToken)!
            .Revoked.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_TokenExists_RevokesToken()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            Revoked = null,
            Expires = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke-token", request);

        // assert
        RefreshDbContext();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await DbContext.Users.FindAsync(testUser.Id))!
            .RefreshTokens.SingleOrDefault(token => token.Token == testToken)!
            .Revoked.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_TokenDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();

        var request = new RevokeRefreshTokenRequest { RefreshToken = "nonExistingToken" };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task RevokeToken_TokenIsExpired_ReturnsBadRequestException()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            Revoked = null,
            Expires = DateTime.UtcNow.Subtract(1.Days())
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task RevokeToken_TokenIsRevoked_ReturnsBadRequestException()
    {
        // arrange
        var client = GetHttpClient();

        const string testToken = "testToken";
        var testUser = new AppUser { UserName = "testUser" };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            Revoked = DateTime.UtcNow.Subtract(1.Days()),
            Expires = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        var request = new RevokeRefreshTokenRequest { RefreshToken = testToken };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/revoke-token", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}