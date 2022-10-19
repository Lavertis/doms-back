using System.Net;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Wrappers;
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
            new() {Id = Guid.NewGuid(), UserName = "user1", FirstName = "", LastName = ""},
            new() {Id = Guid.NewGuid(), UserName = "user2", FirstName = "", LastName = ""},
            new() {Id = Guid.NewGuid(), UserName = "user3", FirstName = "", LastName = ""}
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
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<UserResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        expectedResponse.ForEach(user => { responseContent.Records.Should().ContainEquivalentOf(user); });
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Patient)]
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
    public async void GetAllUsers_NoPaginationProvided_ReturnsAllUsers()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var users = new List<AppUser>();
        for (var i = 0; i < 10; i++)
        {
            users.Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "user1", FirstName = "oldFirstName", LastName = "oldLastName"
            });
        }

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = DbContext.Users.Select(appUser => Mapper.Map<UserResponse>(appUser));

        // act
        var response = await client.GetAsync(UrlPrefix);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<UserResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }


    [Fact]
    public async void GetAllUsers_PaginationProvided_ReturnsAllUsers()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var users = new List<AppUser>();
        for (var i = 0; i < 10; i++)
        {
            users.Add(new AppUser() {Id = Guid.NewGuid(), UserName = "user1", FirstName = "", LastName = ""});
        }

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = DbContext.Users
            .Select(appUser => Mapper.Map<UserResponse>(appUser))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<UserResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
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
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = ""
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
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Patient)]
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