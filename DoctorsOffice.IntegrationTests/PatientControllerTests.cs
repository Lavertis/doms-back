﻿using System.Net;
using System.Text;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class PatientControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/patients";

    public PatientControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAuthenticatedPatient_AuthenticatedUserIsPatient_ReturnsAuthenticatedUser()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var expectedResponse = Mapper.Map<PatientResponse>(
            await DbContext.Patients
                .Include(p => p.AppUser)
                .FirstAsync(p => p.Id == authenticatedPatientId)
        );

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PatientResponse>();

        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Doctor)]
    public async Task GetAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(string role)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, role);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = await DbContext.Patients.FindAsync(authenticatedPatientId);

        DbContext.Patients.Remove(authenticatedPatient!);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePatient_ValidRequest_CreatesPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            NationalId = "123456789",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = "testPassword12345#",
            ConfirmPassword = "testPassword12345#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RefreshDbContext();

        var createdPatient = DbContext.Patients
            .Include(p => p.AppUser)
            .ToList()
            .First(p => p.AppUser.Email == createPatientRequest.Email);

        createdPatient.AppUser.UserName.Should().Be(createPatientRequest.Email);
        createdPatient.AppUser.NormalizedUserName.Should().Be(createPatientRequest.Email.ToUpper());
        createdPatient.AppUser.FirstName.Should().Be(createPatientRequest.FirstName);
        createdPatient.AppUser.LastName.Should().Be(createPatientRequest.LastName);
        createdPatient.AppUser.Email.Should().Be(createPatientRequest.Email);
        createdPatient.AppUser.NormalizedEmail.Should().Be(createPatientRequest.Email.ToUpper());
        createdPatient.AppUser.PhoneNumber.Should().Be(createPatientRequest.PhoneNumber);
        createdPatient.Address.Should().Be(createPatientRequest.Address);
        createdPatient.DateOfBirth.Should().Be(createPatientRequest.DateOfBirth.Date);
    }

    [Theory]
    [InlineData("FirstName", "")]
    [InlineData("LastName", "")]
    [InlineData("Email", "not_correct_email")]
    [InlineData("Email", "word")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Password", "")]
    [InlineData("ConfirmPassword", "")]
    public async Task CreatePatient_SingleFieldInvalid_ReturnsBadRequest(string filedName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = "testPassword12345#",
            ConfirmPassword = "testPassword12345#"
        };

        typeof(CreatePatientRequest).GetProperty(filedName)!.SetValue(createPatientRequest, fieldValue);

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_EmailAlreadyExists_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = "testPassword12345#",
            ConfirmPassword = "testPassword12345#"
        };

        var conflictingPatient = new Patient
        {
            Address = "address",
            NationalId = "123456789",
            AppUser = new AppUser
            {
                UserName = "conflictingPatient",
                Email = createPatientRequest.Email,
                NormalizedEmail = createPatientRequest.Email.ToUpper(),
                FirstName = "firstName",
                LastName = "lastName"
            }
        };

        DbContext.Patients.Add(conflictingPatient);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_PasswordAndConfirmPasswordDontMatch_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = "testPassword12345#",
            ConfirmPassword = "notMatchingPassword12345#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_PasswordIsTooWeak_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = "passwd",
            ConfirmPassword = "passwd"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_ProvidedDateOfBirthHasTimeComponent_StripsTimeComponent()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createPatientRequest = new CreatePatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            NationalId = "123456789",
            DateOfBirth = DateTime.Parse("2020-07-10T04:12:34"),
            Password = "testPassword12345#",
            ConfirmPassword = "testPassword12345#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        RefreshDbContext();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        DbContext.Patients
            .Include(p => p.AppUser)
            .ToList()
            .First(p => p.AppUser.Email == createPatientRequest.Email).DateOfBirth
            .Should().Be(DateTime.Parse("2020-07-10"));
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_ValidRequest_UpdatesPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId =
            await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            NationalId = "987654321",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            NewPassword = "newPassword12345#",
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedPatient = DbContext.Patients
            .Include(p => p.AppUser)
            .FirstOrDefault(p => p.Id == authenticatedPatientId)!;
        updatedPatient.AppUser.UserName.Should().Be(updatePatientRequest.Email);
        updatedPatient.AppUser.NormalizedUserName.Should().Be(updatePatientRequest.Email.ToUpper());
        updatedPatient.AppUser.FirstName.Should().Be(updatePatientRequest.FirstName);
        updatedPatient.AppUser.LastName.Should().Be(updatePatientRequest.LastName);
        updatedPatient.AppUser.Email.Should().Be(updatePatientRequest.Email);
        updatedPatient.AppUser.NormalizedEmail.Should().Be(updatePatientRequest.Email.ToUpper());
        updatedPatient.AppUser.PhoneNumber.Should().Be(updatePatientRequest.PhoneNumber);
        updatedPatient.Address.Should().Be(updatePatientRequest.Address);
        updatedPatient.DateOfBirth.Should().Be(updatePatientRequest.DateOfBirth.Value.Date);
        updatedPatient.AppUser.PasswordHash.Should().NotBe(oldPasswordHash);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_EmailAlreadyExists_ReturnsBadRequest()
    {
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var conflictPatient = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName2",
            LastName = "oldLastName2",
            Email = "oldEmail2@mail.com",
            NationalId = "987654321",
            Address = "oldAddress2",
            PhoneNumber = "123457789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            Email = conflictPatient.AppUser.Email,
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_CurrentPasswordDoesntMatch_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            FirstName = "testFirstName",
            CurrentPassword = "notMatchingPassword12345#"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_NewPasswordTooWeak_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            NewPassword = "passwd",
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Doctor)]
    public async Task UpdateAuthenticatedPatient_AuthenticatedUserOtherThanPatient_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();

        await AuthenticateAsRoleAsync(client, roleName);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            FirstName = "newFirstName",
            CurrentPassword = "patientPassword"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            FirstName = "newFirstName",
            CurrentPassword = "patientPassword"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        var client = await GetHttpClientAsync();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            LastName = "newLastName",
            CurrentPassword = patientPassword
        };

        DbContext.Patients.Remove(patientToBeUpdated);
        await DbContext.SaveChangesAsync();

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("FirstName", "NewFirstName")]
    [InlineData("LastName", "NewLastName")]
    [InlineData("Address", "NewAddress")]
    [InlineData("DateOfBirth", "2002-07-10")]
    [InlineData("NewPassword", "NewPassword12345#")]
    [InlineData("PhoneNumber", "999999999")]
    [InlineData("Email", "newMail@mail.com")]
    [InlineData("NationalId", "123456789")]
    public async Task UpdateAuthenticatedPatient_SingleFiledPresent_UpdatesField(string fieldName, string fieldValue)
    {
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId =
            await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            CurrentPassword = patientPassword
        };
        if (fieldName == "DateOfBirth")
        {
            updatePatientRequest.DateOfBirth = DateTime.Parse(fieldValue);
        }
        else
        {
            typeof(UpdateAuthenticatedPatientRequest).GetProperty(fieldName)!.SetValue(updatePatientRequest,
                fieldValue);
        }

        DbContext.Patients.Remove(patientToBeUpdated);

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedPatient = DbContext.Patients
            .Include(p => p.AppUser)
            .First(p => p.Id == authenticatedPatientId);

        switch (fieldName)
        {
            case "FirstName":
                updatedPatient.AppUser.FirstName.Should().Be(fieldValue);
                break;
            case "LastName":
                updatedPatient.AppUser.LastName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedPatient.AppUser.Email.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedEmail.Should().Be(fieldValue.ToUpper());
                updatedPatient.AppUser.UserName.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedUserName.Should().Be(fieldValue.ToUpper());
                break;
            case "NationalId":
                updatedPatient.NationalId.Should().Be(fieldValue);
                break;
            case "Address":
                updatedPatient.Address.Should().Be(fieldValue);
                break;
            case "DateOfBirth":
                updatedPatient.DateOfBirth.Should().Be(DateTime.Parse(fieldValue));
                break;
            case "PhoneNumber":
                updatedPatient.AppUser.PhoneNumber.Should().Be(fieldValue);
                break;
        }
    }

    [Theory]
    [InlineData("FirstName", "")]
    [InlineData("FirstName", "aa")]
    [InlineData("LastName", "")]
    [InlineData("LastName", "aa")]
    [InlineData("Address", "")]
    [InlineData("Address", "aa")]
    [InlineData("NewPassword", "")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Email", "")]
    [InlineData("Email", "aaa")]
    [InlineData("NationalId", "")]
    public async Task UpdateAuthenticatedPatient_SingleInvalidField_ReturnsBadRequest(string fieldName,
        string fieldValue)
    {
        var client = await GetHttpClientAsync();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "987654321",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId =
            await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            CurrentPassword = patientPassword
        };

        typeof(UpdateAuthenticatedPatientRequest).GetProperty(fieldName)!.SetValue(updatePatientRequest, fieldValue);


        DbContext.Patients.Remove(patientToBeUpdated);

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        RefreshDbContext();
        var updatedPatient = DbContext.Patients
            .Include(p => p.AppUser)
            .First(p => p.Id == authenticatedPatientId);
        switch (fieldName)
        {
            case "FirstName":
                updatedPatient.AppUser.FirstName.Should().NotBe(fieldValue);
                break;
            case "LastName":
                updatedPatient.AppUser.LastName.Should().NotBe(fieldValue);
                break;
            case "Email":
                updatedPatient.AppUser.Email.Should().NotBe(fieldValue);
                updatedPatient.AppUser.UserName.Should().NotBe(fieldValue);
                break;
            case "NationalId":
                updatedPatient.NationalId.Should().NotBe(fieldValue);
                break;
            case "Address":
                updatedPatient.Address.Should().NotBe(fieldValue);
                break;
            case "DateOfBirth":
                updatedPatient.DateOfBirth.Should().NotBe(DateTime.Parse(fieldValue));
                break;
            case "PhoneNumber":
                updatedPatient.AppUser.PhoneNumber.Should().NotBe(fieldValue);
                break;
        }
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_ProvidedDateOfBirthHasTimeComponent_StripsTimeComponent()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientRequest
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            NationalId = "123456789",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId =
            await AuthenticateAsAsync(client, patientToBeUpdated.AppUser.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            DateOfBirth = DateTime.Parse("2000-07-10T04:12:34Z"),
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/current", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        (await DbContext.Patients.FindAsync(authenticatedPatientId))!.DateOfBirth
            .Should().Be(DateTime.Parse("2000-07-10"));
    }

    [Fact]
    public async Task DeleteAuthenticatedPatient_AuthenticatedUserIsPatient_DeletesAuthenticatedUser()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        (await DbContext.Users.AnyAsync(p => p.Id == authenticatedPatientId)).Should().BeFalse();
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Doctor)]
    public async Task DeleteAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act  
        var response = await client.DeleteAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act  
        var response = await client.DeleteAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        DbContext.Users.Remove(DbContext.Users.First(p => p.Id == authenticatedPatientId));
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Patient> CreatePatient(CreatePatientRequest createPatientCommand)
    {
        var hasher = new PasswordHasher<AppUser>();

        var newPatient = new Patient
        {
            Address = createPatientCommand.Address,
            DateOfBirth = createPatientCommand.DateOfBirth,
            NationalId = createPatientCommand.NationalId,
            AppUser = new AppUser
            {
                UserName = createPatientCommand.Email,
                NormalizedUserName = createPatientCommand.Email.ToUpper(),
                Email = createPatientCommand.Email,
                NormalizedEmail = createPatientCommand.Email.ToUpper(),
                EmailConfirmed = true,
                FirstName = createPatientCommand.FirstName,
                LastName = createPatientCommand.LastName,
                PhoneNumber = createPatientCommand.PhoneNumber,
                PasswordHash = hasher.HashPassword(null!, createPatientCommand.Password),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        };
        DbContext.Patients.Add(newPatient);
        var patientRoleId = DbContext.Roles.FirstOrDefault(r => r.Name == Roles.Patient)!.Id;
        DbContext.IdentityUserRole.Add(new IdentityUserRole<Guid>
        {
            UserId = newPatient.AppUser.Id,
            RoleId = patientRoleId
        });
        await DbContext.SaveChangesAsync();

        return newPatient;
    }
}