﻿using System.Net;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Wrappers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class AdminControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/admins";

    public AdminControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserIsAdmin_ReturnsAuthenticatedAdmin()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedAdminId = await AuthenticateAsAdminAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminResponse = await response.Content.ReadAsAsync<AdminResponse>();

        adminResponse.Id.Should().Be(authenticatedAdminId);
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedAdminId = await AuthenticateAsAdminAsync(client);
        var authenticatedAdmin = await DbContext.Admins.FindAsync(authenticatedAdminId);

        DbContext.Admins.Remove(authenticatedAdmin!);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuthenticatedAdmin_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(Roles.Doctor)]
    public async Task GetAuthenticatedAdmin_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminById_AdminWithSpecifiedIdExist_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var admin = new Admin
        {
            AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}
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
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var nonExistingAdminId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAdminById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var nonExistingAdminId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(Roles.Doctor)]
    public async Task GetAdminById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var nonExistingAdminId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/{nonExistingAdminId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAdmins_AdminsExist_ReturnsAllAdmins()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var admins = new List<Admin>
        {
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}}
        };

        DbContext.Admins.AddRange(admins);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AdminResponse>>();

        foreach (var admin in admins)
            responseContent.Records.Should().ContainEquivalentOf(new AdminResponse {Id = admin.Id});
    }

    [Fact]
    public async Task GetAllAdmins_NoAdmins_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        DbContext.Admins.RemoveRange(DbContext.Admins);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AdminResponse>>();

        responseContent.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAdmins_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(Roles.Doctor)]
    public async Task GetAllAdmins_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAdmins_NoPaginationProvided_ReturnsAllAdmins()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var admins = new List<Admin>
        {
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}}
        };

        DbContext.Admins.AddRange(admins);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AdminResponse>>();

        foreach (var admin in admins)
            responseContent.Records.Should().ContainEquivalentOf(new AdminResponse {Id = admin.Id});
    }

    [Fact]
    public async Task GetAllAdmins_PaginationProvided_ReturnsRequestedPage()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var admins = new List<Admin>();
        for (var i = 0; i < 20; i++)
        {
            admins.Add(new Admin {AppUser = new AppUser {Id = Guid.NewGuid(), FirstName = "", LastName = ""}});
        }

        DbContext.Admins.AddRange(admins);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = await DbContext.Admins
            .Select(a => Mapper.Map<AdminResponse>(a))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AdminResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }
}