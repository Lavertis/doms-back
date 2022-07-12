using System.Net;
using System.Text;
using DoctorsOfficeApi.CQRS.Commands.CreatePatient;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

public class PatientControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/patient";

    public PatientControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAuthenticatedPatient_AuthenticatedUserIsPatient_ReturnsAuthenticatedUser()
    {
        // arrange
        var client = GetHttpClient();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var expectedResponse = new PatientResponse((await DbContext.Patients.FindAsync(authenticatedPatientId))!);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PatientResponse>();

        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(string roleType)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRoleAsync(client, roleType);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = await DbContext.Patients.FindAsync(authenticatedPatientId);

        DbContext.Patients.Remove(authenticatedPatient!);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePatient_ValidRequest_CreatesPatient()
    {
        // arrange
        var client = GetHttpClient();

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
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

        var createdPatient = DbContext.Patients.ToList().First(p => p.UserName == createPatientRequest.UserName)!;

        createdPatient.UserName.Should().Be(createPatientRequest.UserName);
        createdPatient.AppUser.NormalizedUserName.Should().Be(createPatientRequest.UserName.ToUpper());
        createdPatient.FirstName.Should().Be(createPatientRequest.FirstName);
        createdPatient.LastName.Should().Be(createPatientRequest.LastName);
        createdPatient.Email.Should().Be(createPatientRequest.Email);
        createdPatient.AppUser.NormalizedEmail.Should().Be(createPatientRequest.Email.ToUpper());
        createdPatient.PhoneNumber.Should().Be(createPatientRequest.PhoneNumber);
        createdPatient.Address.Should().Be(createPatientRequest.Address);
        createdPatient.DateOfBirth.Should().Be(createPatientRequest.DateOfBirth.Date);
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("UserName", "aa")]
    [InlineData("FirstName", "")]
    [InlineData("LastName", "")]
    [InlineData("Email", "")]
    [InlineData("Email", "aaaaaaa")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Password", "")]
    [InlineData("ConfirmPassword", "")]
    public async Task CreatePatient_SingleFieldInvalid_ReturnsBadRequest(string filedName, string fieldValue)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
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
    public async Task CreatePatient_UserNameAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
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
            FirstName = "firstName",
            LastName = "lastName",
            Address = "address",
            AppUser = new AppUser
            {
                UserName = createPatientRequest.UserName,
                NormalizedUserName = createPatientRequest.UserName.ToUpper()
            }
        };

        DbContext.Patients.Add(conflictingPatient);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreatePatient_EmailAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
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
            FirstName = "firstName",
            LastName = "lastName",
            Address = "address",
            AppUser = new AppUser
            {
                UserName = "conflictingPatient",
                Email = createPatientRequest.Email,
                NormalizedEmail = createPatientRequest.Email.ToUpper()
            }
        };

        DbContext.Patients.Add(conflictingPatient);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreatePatient_PasswordAndConfirmPasswordDontMatch_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
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
        var client = GetHttpClient();
        await AuthenticateAsPatientAsync(client);

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
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
        var client = GetHttpClient();

        var createPatientRequest = new CreatePatientRequest
        {
            UserName = "testUserName",
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.Parse("2020-07-10T04:12:34"),
            Password = "testPassword12345#",
            ConfirmPassword = "testPassword12345#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", createPatientRequest);

        // assert
        RefreshDbContext();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        DbContext.Patients.ToList().First(p => p.UserName == createPatientRequest.UserName).DateOfBirth
            .Should().Be(DateTime.Parse("2020-07-10"));
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_ValidRequest_UpdatesPatient()
    {
        // arrange
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId = await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            UserName = "testUserName",
            FirstName = "testFirstName",
            LastName = "testLastName",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "testAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            NewPassword = "newPassword12345#",
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedPatient = DbContext.Patients.FirstOrDefault(p => p.Id == authenticatedPatientId)!;
        updatedPatient.UserName.Should().Be(updatePatientRequest.UserName);
        updatedPatient.AppUser.NormalizedUserName.Should().Be(updatePatientRequest.UserName.ToUpper());
        updatedPatient.FirstName.Should().Be(updatePatientRequest.FirstName);
        updatedPatient.LastName.Should().Be(updatePatientRequest.LastName);
        updatedPatient.Email.Should().Be(updatePatientRequest.Email);
        updatedPatient.AppUser.NormalizedEmail.Should().Be(updatePatientRequest.Email.ToUpper());
        updatedPatient.PhoneNumber.Should().Be(updatePatientRequest.PhoneNumber);
        updatedPatient.Address.Should().Be(updatePatientRequest.Address);
        updatedPatient.DateOfBirth.Should().Be(updatePatientRequest.DateOfBirth.Value.Date);
        updatedPatient.AppUser.PasswordHash.Should().NotBe(oldPasswordHash);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_UserNameAlreadyExists_ReturnsConflict()
    {
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var conflictPatient = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName2",
            FirstName = "oldFirstName2",
            LastName = "oldLastName2",
            Email = "oldEmail2@mail.com",
            Address = "oldAddress2",
            PhoneNumber = "123455789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            UserName = conflictPatient.UserName,
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_EmailAlreadyExists_ReturnsConflict()
    {
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var conflictPatient = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName2",
            FirstName = "oldFirstName2",
            LastName = "oldLastName2",
            Email = "oldEmail2@mail.com",
            Address = "oldAddress2",
            PhoneNumber = "123457789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            Email = conflictPatient.Email,
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_CurrentPasswordDoesntMatch_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            FirstName = "testFirstName",
            CurrentPassword = "notMatchingPassword12345#"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_NewPasswordTooWeak_ReturnsBadRequest()
    {
        // arrange
        var client = GetHttpClient();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            NewPassword = "passwd",
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Doctor)]
    public async Task UpdateAuthenticatedPatient_AuthenticatedUserOtherThanPatient_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();

        await AuthenticateAsRoleAsync(client, roleName);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            FirstName = "newFirstName",
            CurrentPassword = "patientPassword"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            FirstName = "newFirstName",
            CurrentPassword = "patientPassword"
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        var client = GetHttpClient();

        const string patientPassword = "oldPassword12345#";

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

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
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        var responseContent = await response.Content.ReadAsStringAsync();

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
    [InlineData("UserName", "newUserName")]
    [InlineData("Email", "newMail@mail.com")]
    public async Task UpdateAuthenticatedPatient_SingleFiledPresent_UpdatesField(string fieldName, string fieldValue)
    {
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId = await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

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
            typeof(UpdateAuthenticatedPatientRequest).GetProperty(fieldName)!.SetValue(updatePatientRequest, fieldValue);
        }

        DbContext.Patients.Remove(patientToBeUpdated);

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedPatient = DbContext.Patients.First(p => p.Id == authenticatedPatientId);

        switch (fieldName)
        {
            case "FirstName":
                updatedPatient.FirstName.Should().Be(fieldValue);
                break;
            case "LastName":
                updatedPatient.LastName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedPatient.AppUser.Email.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedEmail.Should().Be(fieldValue.ToUpper());
                break;
            case "Address":
                updatedPatient.Address.Should().Be(fieldValue);
                break;
            case "DateOfBirth":
                updatedPatient.DateOfBirth.Should().Be(DateTime.Parse(fieldValue));
                break;
            case "UserName":
                updatedPatient.AppUser.UserName.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedUserName.Should().Be(fieldValue.ToUpper());
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
    [InlineData("UserName", "")]
    [InlineData("Email", "")]
    [InlineData("Email", "aaa")]
    public async Task UpdateAuthenticatedPatient_SingleInvalidField_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId = await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest
        {
            CurrentPassword = patientPassword
        };

        typeof(UpdateAuthenticatedPatientRequest).GetProperty(fieldName)!.SetValue(updatePatientRequest, fieldValue);


        DbContext.Patients.Remove(patientToBeUpdated);

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        RefreshDbContext();
        var updatedPatient = DbContext.Patients.First(p => p.Id == authenticatedPatientId);
        switch (fieldName)
        {
            case "FirstName":
                updatedPatient.FirstName.Should().NotBe(fieldValue);
                break;
            case "LastName":
                updatedPatient.LastName.Should().NotBe(fieldValue);
                break;
            case "Email":
                updatedPatient.AppUser.Email.Should().NotBe(fieldValue);
                break;
            case "Address":
                updatedPatient.Address.Should().NotBe(fieldValue);
                break;
            case "DateOfBirth":
                updatedPatient.DateOfBirth.Should().NotBe(DateTime.Parse(fieldValue));
                break;
            case "UserName":
                updatedPatient.AppUser.UserName.Should().NotBe(fieldValue);
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
        var client = GetHttpClient();

        var hasher = new PasswordHasher<AppUser>();
        const string patientPassword = "oldPassword12345#";
        var oldPasswordHash = hasher.HashPassword(new AppUser(), patientPassword);

        var patientToBeUpdated = await CreatePatient(new CreatePatientCommand
        {
            UserName = "oldUserName",
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Email = "oldEmail@mail.com",
            Address = "oldAddress",
            PhoneNumber = "123456789",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            Password = patientPassword
        });

        var authenticatedPatientId = await AuthenticateAsAsync(client, patientToBeUpdated.UserName, patientPassword);

        var updatePatientRequest = new UpdateAuthenticatedPatientRequest()
        {
            DateOfBirth = DateTime.Parse("2000-07-10T04:12:34Z"),
            CurrentPassword = patientPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updatePatientRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

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
        var client = GetHttpClient();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        (await DbContext.Patients.AnyAsync(p => p.Id == authenticatedPatientId)).Should().BeFalse();
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Doctor)]
    public async Task DeleteAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = GetHttpClient();
        await AuthenticateAsRoleAsync(client, roleName);

        // act  
        var response = await client.DeleteAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = GetHttpClient();

        // act  
        var response = await client.DeleteAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAuthenticatedPatient_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = GetHttpClient();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        DbContext.Patients.Remove(DbContext.Patients.First(p => p.Id == authenticatedPatientId));
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Patient> CreatePatient(CreatePatientCommand createPatientCommand)
    {
        var hasher = new PasswordHasher<AppUser>();

        var newPatient = new Patient
        {
            FirstName = createPatientCommand.FirstName,
            LastName = createPatientCommand.LastName,
            Address = createPatientCommand.Address,
            DateOfBirth = createPatientCommand.DateOfBirth,
            AppUser = new AppUser
            {
                UserName = createPatientCommand.UserName,
                NormalizedUserName = createPatientCommand.UserName.ToUpper(),
                Email = createPatientCommand.Email,
                NormalizedEmail = createPatientCommand.Email.ToUpper(),
                PhoneNumber = createPatientCommand.PhoneNumber,
                PasswordHash = hasher.HashPassword(null!, createPatientCommand.Password)
            }
        };
        DbContext.Patients.Add(newPatient);
        var patientRoleId = DbContext.Roles.FirstOrDefault(r => r.Name == RoleTypes.Patient)!.Id;
        DbContext.IdentityUserRole.Add(new IdentityUserRole<string>
        {
            UserId = newPatient.AppUser.Id,
            RoleId = patientRoleId,
        });
        await DbContext.SaveChangesAsync();

        return newPatient;
    }
}