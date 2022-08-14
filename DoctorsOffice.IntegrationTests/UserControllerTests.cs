using System.Net;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class UserControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/users";

    public UserControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async void GetAllUsers_AuthenticatedUserIsAdmin_ReturnsAllUsers()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var users = new List<AppUser>
        {
            new() {Id = Guid.NewGuid(), UserName = "user1"},
            new() {Id = Guid.NewGuid(), UserName = "user2"},
            new() {Id = Guid.NewGuid(), UserName = "user3"}
        };
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var expectedResponse = users
            .Select(appUser => new UserResponse
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                NormalizedUserName = appUser.NormalizedUserName
            }).ToList();

        // act
        var response = await client.GetAsync(UrlPrefix);

        // assert
        var responseContent = await response.Content.ReadAsAsync<List<UserResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        expectedResponse.ForEach(user => { responseContent.Should().ContainEquivalentOf(user); });
    }

    [Theory]
    [InlineData(RoleTypes.Doctor)]
    [InlineData(RoleTypes.Patient)]
    public async void GetAllUsers_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync(UrlPrefix);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_RequestedUserExists_ReturnsUserWithSpecifiedId()
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

        var expectedResponse = new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName
        };

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
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var nonExistingUserId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(RoleTypes.Doctor)]
    [InlineData(RoleTypes.Patient)]
    public async Task GetUserById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var dummyUserId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{dummyUserId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}