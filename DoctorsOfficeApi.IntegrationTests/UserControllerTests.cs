using System.Net;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

public class UserControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/user";

    public UserControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async void GetAllUsers_AuthenticatedUserIsAdmin_ReturnsAllUsers()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        var users = new List<AppUser>
        {
            new() { UserName = "user1" },
            new() { UserName = "user2" },
            new() { UserName = "user3" },
        };
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var expectedResponse = users.Select(u => new UserResponse(u)).ToList();

        // act
        var response = await client.GetAsync(UrlPrefix);

        // assert
        var responseContent = await response.Content.ReadAsAsync<List<UserResponse>>();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        expectedResponse.ForEach(user => { responseContent.Should().ContainEquivalentOf(user); });
    }

    [Theory]
    [InlineData(RoleType.Doctor)]
    [InlineData(RoleType.Patient)]
    public async void GetAllUsers_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRole(client, roleName);

        // act
        var response = await client.GetAsync(UrlPrefix);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_RequestedUserExists_ReturnsUserWithSpecifiedId()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        var user = new AppUser
        {
            UserName = "user1",
            Id = "user1Id"
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var expectedResponse = new UserResponse(user);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{user.Id}");

        // assert
        var responseContent = await response.Content.ReadAsAsync<UserResponse>();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetUserById_RequestedUserDoesNotExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        const string nonExistingUserId = "nonExistingUserId";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(RoleType.Doctor)]
    [InlineData(RoleType.Patient)]
    public async Task GetUserById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRole(client, roleName);

        const string dummyUserId = "dummyUserId";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{dummyUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRefreshTokensByUserId_RequestedUserExists_ReturnsAllRefreshTokens()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        var user = new AppUser
        {
            UserName = "user1",
            Id = "user1Id"
        };
        var refreshTokens = new List<RefreshToken>
        {
            new() { Id = 100, Token = "token1" },
            new() { Id = 101, Token = "token2" },
            new() { Id = 102, Token = "token3" },
        };
        user.RefreshTokens.AddRange(refreshTokens);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{user.Id}/refresh-tokens");
        
        // assert
        var responseContent = await response.Content.ReadAsAsync<List<RefreshToken>>();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().BeEquivalentTo(refreshTokens);
    }
    
    [Fact]
    public async Task GetRefreshTokensByUserId_RequestedUserDoesNotExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);
        
        const string nonExistingUserId = "nonExistingUserId";
        
        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingUserId}/refresh-tokens");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }   
    
    [Fact]
    public async Task GetRefreshTokensByUserId_UserDoesntHaveRefreshTokens_ReturnsEmptyList()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);
        
        var user = new AppUser
        {
            UserName = "user1",
            Id = "user1Id"
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        
        // act
        var response = await client.GetAsync($"{UrlPrefix}/{user.Id}/refresh-tokens");
        
        // assert
        var responseContent = await response.Content.ReadAsAsync<List<RefreshToken>>();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(RoleType.Doctor)]
    [InlineData(RoleType.Patient)]
    public async Task GetRefreshTokensByUserId_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRole(client, roleName);

        const string dummyUserId = "dummyUserId";
        
        // act
        var response = await client.GetAsync($"{UrlPrefix}/{dummyUserId}/refresh-tokens");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}