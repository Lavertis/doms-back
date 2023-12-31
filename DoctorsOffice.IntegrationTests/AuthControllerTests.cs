using System.Net;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
            FirstName = "",
            LastName = "",
            PasswordHash = hasher.HashPassword(null!, testPassword),
            SecurityStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = true
        });
        await DbContext.SaveChangesAsync();

        var request = new AuthenticateRequest
        {
            Login = testUserName,
            Password = testPassword
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/authenticate", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<AuthenticateResponse>();
        responseContent.JwtToken.Should().NotBeNullOrEmpty();
        var setCookieValue = response.Headers.GetValues("Set-Cookie").First();
        setCookieValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authenticate_UserNameDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var request = new AuthenticateRequest
        {
            Login = "nonExistingUser",
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
            NormalizedUserName = testUserName.ToUpper(), FirstName = "", LastName = "",
            PasswordHash = hasher.HashPassword(null!, testPassword)
        });
        await DbContext.SaveChangesAsync();

        var request = new AuthenticateRequest
        {
            Login = testUserName,
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
            SecurityStamp = Guid.NewGuid().ToString(), FirstName = "", LastName = ""
        };
        testUser.RefreshTokens.Add(new RefreshToken
        {
            Token = testToken,
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        DbContext.Users.Add(testUser);
        await DbContext.SaveChangesAsync();

        client.DefaultRequestHeaders.Add("Cookie", $"{Cookies.RefreshToken}={Uri.EscapeDataString(testToken)}");

        // act
        var response = await client.PostAsync($"{UrlPrefix}/refresh-token", null);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<AuthenticateResponse>();
        responseContent.JwtToken.Should().NotBeNullOrEmpty();
        var setCookieValue = response.Headers.GetValues("Set-Cookie").First();
        setCookieValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_NonExistingToken_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        client.DefaultRequestHeaders.Add("Cookie", $"{Cookies.RefreshToken}=nonExistingToken");

        // act
        var response = await client.PostAsync($"{UrlPrefix}/refresh-token", null);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_TokenIsExpired_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string testToken = "testToken";
        var testUser = new AppUser {UserName = "testUser", FirstName = "", LastName = ""};
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
            SecurityStamp = Guid.NewGuid().ToString(), FirstName = "", LastName = ""
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
            SecurityStamp = Guid.NewGuid().ToString(), FirstName = "", LastName = ""
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
        var user = await DbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstAsync(user => user.Id == testUser.Id);
        user.RefreshTokens.Single(token => token.Token == descendantToken).Should().NotBeNull();
    }
}