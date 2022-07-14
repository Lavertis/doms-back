using System.Net;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

public class AdminControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/admin";

    public AdminControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserIsAdmin_ReturnsAuthenticatedAdmin()
    {
        // arrange
        var client = GetHttpClient();
        var authenticatedAdminId = await AuthenticateAsAdminAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<AdminResponse>();

        responseContent.Id.Should().Be(authenticatedAdminId);
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserDoesntExist_ReturnsNotFoundException()
    {
        // arrange
        var client = GetHttpClient();
        var authenticatedAdminId = await AuthenticateAsAdminAsync(client);
        var authenticatedAdmin = await DbContext.Admins.FindAsync(authenticatedAdminId);

        DbContext.Admins.Remove(authenticatedAdmin!);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminById_AdminWithSpecifiedIdExist_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        var admin = new Admin
        {
            AppUser = new AppUser { Id = "100" }
        };

        DbContext.Admins.Add(admin);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{admin.AppUser.Id}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<AdminResponse>();

        responseContent.Id.Should().Be(admin.AppUser.Id);
    }

    [Fact]
    public async Task GetAdminById_AdminWithSpecifiedIdDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        const string nonExistingAdminId = "100";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAdminById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        const string nonExistingAdminId = "100";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetAdminById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRoleAsync(client, roleName);

        const string nonExistingAdminId = "100";

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAdmins_ThereAreAdmins_ReturnsAllAdmins()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        var admins = new List<Admin>
        {
            new() { AppUser = new AppUser { Id = "100" } },
            new() { AppUser = new AppUser { Id = "200" } },
            new() { AppUser = new AppUser { Id = "300" } }
        };

        DbContext.Admins.AddRange(admins);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<AdminResponse>>();

        foreach (var admin in admins)
            responseContent.Should().ContainEquivalentOf(new AdminResponse(admin));
    }

    [Fact]
    public async Task GetAllAdmins_ThereAreNoAdmins_ReturnsEmptyList()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsAdminAsync(client);

        DbContext.Admins.RemoveRange(DbContext.Admins);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<AdminResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAdmins_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetAllAdmins_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}