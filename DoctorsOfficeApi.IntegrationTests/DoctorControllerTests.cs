using System.Net;
using System.Text;
using DoctorsOfficeApi.CQRS.Commands.CreateDoctor;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOfficeApi.IntegrationTests;

public class DoctorControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/doctor";

    public DoctorControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAuthenticatedDoctor_AuthenticatedUserIsDoctor_ReturnsAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var authenticatedDoctor = await DbContext.Doctors.FindAsync(authenticatedDoctorId);

        var expectedResponse = new DoctorResponse(authenticatedDoctor!);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<DoctorResponse>();

        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAuthenticatedDoctor_AuthenticatedUserDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var authenticatedDoctor = await DbContext.Doctors.FindAsync(authenticatedDoctorId);

        DbContext.Doctors.Remove(authenticatedDoctor!);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuthenticatedDoctor_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Admin)]
    public async Task GetAuthenticatedDoctor_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/auth");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllDoctors_AuthenticatedUserIsAdmin_ReturnsAllDoctors()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        for (var i = 0; i < 3; i++)
            DbContext.Doctors.Add(new Doctor { AppUser = new AppUser() });

        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<IList<DoctorResponse>>();

        responseContent.Count.Should().Be(DbContext.Doctors.Count());
        responseContent.Should().BeEquivalentTo(DbContext.Doctors.Select(d => new DoctorResponse(d)));
    }

    [Fact]
    public async Task GetAllDoctors_NoDoctors_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        DbContext.Doctors.RemoveRange(DbContext.Doctors);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<IList<DoctorResponse>>();

        responseContent.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetAllDoctors_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetAllDoctors_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
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
    public async Task CreateDoctor_ValidRequest_CreatesDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var request = new CreateDoctorRequest
        {
            Email = "doctor@mail.com",
            UserName = "doctorUserName",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RefreshDbContext();

        var createdDoctor = (await DbContext.Doctors.ToListAsync()).First(d => d.UserName == request.UserName);
        createdDoctor.Should().NotBeNull();
        createdDoctor.Email.Should().Be(request.Email);
        createdDoctor.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("UserName", "a")]
    [InlineData("Email", "")]
    [InlineData("Email", "a")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Password", "")]
    [InlineData("Password", "pass")]
    public async Task CreateDoctor_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var request = new CreateDoctorRequest
        {
            UserName = "doctorUserName",
            Email = "doctor@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        typeof(CreateDoctorRequest).GetProperty(fieldName)!.SetValue(request, fieldValue);

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDoctor_UserNameAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var request = new CreateDoctorRequest
        {
            UserName = "doctorUserName",
            Email = "doctor@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        var conflictingDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                UserName = request.UserName,
                NormalizedUserName = request.UserName.ToUpper()
            }
        };
        DbContext.Doctors.Add(conflictingDoctor);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateDoctor_EmailAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var request = new CreateDoctorRequest
        {
            UserName = "doctorUserName",
            Email = "doctor@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        var conflictingDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper()
            }
        };
        DbContext.Doctors.Add(conflictingDoctor);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        var str = response.Content.ReadAsStringAsync().Result;
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateDoctor_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var request = new CreateDoctorRequest
        {
            UserName = "doctorUserName",
            Email = "doctor@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Patient)]
    [InlineData(RoleTypes.Doctor)]
    public async Task CreateDoctor_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var request = new CreateDoctorRequest
        {
            UserName = "doctorUserName",
            Email = "doctor@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_ValidRequest_UpdatesAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        var authenticatedDoctorId = await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            UserName = "newDocUserName",
            Email = "newDoctorEmail@mail.com",
            PhoneNumber = "123456789",
            NewPassword = "newPassword1234#",
            ConfirmPassword = "newPassword1234#",
            CurrentPassword = oldDoctorPassword
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedDoctor = DbContext.Doctors.ToList().First(d => d.Id == authenticatedDoctorId);
        updatedDoctor.Should().NotBeNull();
        updatedDoctor.AppUser.UserName.Should().Be(updateDoctorRequest.UserName);
        updatedDoctor.AppUser.Email.Should().Be(updateDoctorRequest.Email);
        updatedDoctor.AppUser.PhoneNumber.Should().Be(updateDoctorRequest.PhoneNumber);
    }

    [Theory]
    [InlineData("UserName", "newDocUserName")]
    [InlineData("Email", "newDoctorEmail@mail.com")]
    [InlineData("PhoneNumber", "987654321")]
    public async Task UpdateAuthenticatedDoctor_SingleFieldProvided_UpdatesProvidedField(
        string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        var authenticatedDoctorId = await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            CurrentPassword = oldDoctorPassword
        };

        typeof(UpdateAuthenticatedDoctorRequest).GetProperty(fieldName)!.SetValue(updateDoctorRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedDoctor = await DbContext.Doctors.FirstAsync(d => d.Id == authenticatedDoctorId);
        updatedDoctor.Should().NotBeNull();

        switch (fieldName)
        {
            case "UserName":
                updatedDoctor.AppUser.UserName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedDoctor.AppUser.Email.Should().Be(fieldValue);
                break;
            case "PhoneNumber":
                updatedDoctor.AppUser.PhoneNumber.Should().Be(fieldValue);
                break;
            default:
                throw new Exception("No assertion for fieldName: " + fieldName);
        }
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("UserName", "a")]
    [InlineData("Email", "")]
    [InlineData("Email", "a")]
    [InlineData("PhoneNumber", "")]
    public async Task UpdateAuthenticatedDoctor_SingleFiledInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            CurrentPassword = oldDoctorPassword
        };

        typeof(UpdateAuthenticatedDoctorRequest).GetProperty(fieldName)!.SetValue(updateDoctorRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_UserNameAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string conflictingUserName = "conflictingUserName";

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        var conflictingDoctor = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = conflictingUserName,
            Email = "mail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            UserName = conflictingUserName,
            CurrentPassword = oldDoctorPassword
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);


        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_EmailAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string conflictingEmail = "conflictingMail@mail.com";

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        var conflictingDoctor = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "userName",
            Email = conflictingEmail,
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);


        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            Email = conflictingEmail,
            CurrentPassword = oldDoctorPassword
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);


        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_ConfirmPasswordDoesntMatch_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword,
        });

        await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            NewPassword = "newPassword1234$",
            ConfirmPassword = "notMatchingPassword1234#",
            CurrentPassword = oldDoctorPassword
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_CurrentPasswordIncorrect_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();

        const string oldDoctorPassword = "oldPassword1234#";
        var doctorToUpdate = await CreateDoctorAsync(new CreateDoctorCommand
        {
            UserName = "oldUserName",
            Email = "oldMail@mail.com",
            PhoneNumber = "123456789",
            Password = oldDoctorPassword
        });

        await AuthenticateAsAsync(client, doctorToUpdate.AppUser.UserName, oldDoctorPassword);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            UserName = "newUserName",
            CurrentPassword = "incorrectPassword"
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthenticatedDoctor_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            UserName = "newUserName",
            CurrentPassword = "incorrectPassword"
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Patient)]
    public async Task UpdateAuthenticatedDoctor_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var updateDoctorRequest = new UpdateAuthenticatedDoctorRequest
        {
            UserName = "newUserName",
            CurrentPassword = "password"
        };
        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/auth", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateDoctorById_DoctorExists_UpdatesDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Doctors.Add(doctorToUpdate);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            NewPassword = "newPassword1234$"
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedDoctor = DbContext.Doctors.First(d => d.AppUser.Id == doctorId);
        updatedDoctor.Should().NotBeNull();
        updatedDoctor.AppUser.UserName.Should().Be(updateDoctorRequest.UserName);
        updatedDoctor.AppUser.Email.Should().Be(updateDoctorRequest.Email);
        updatedDoctor.AppUser.PhoneNumber.Should().Be(updateDoctorRequest.PhoneNumber);
        updatedDoctor.AppUser.PasswordHash.Should().NotBe(oldPasswordHash);
    }

    [Fact]
    public async Task UpdateDoctorById_UserWithSpecifiedIdIsNotDoctor_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var patientId = Guid.NewGuid();
        var patient = new Patient()
        {
            FirstName = "firstName",
            LastName = "lastName",
            Address = "address",
            AppUser = new AppUser
            {
                Id = patientId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Patients.Add(patient);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            NewPassword = "newPassword1234$"
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{patientId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDoctorById_UserWithSpecifiedIdDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var doctorId = Guid.NewGuid();

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            NewPassword = "newPassword1234$"
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("UserName", "newUserName")]
    [InlineData("Email", "oldMail@mail.com")]
    [InlineData("PhoneNumber", "123456789")]
    [InlineData("NewPassword", "newPassword1234$")]
    public async Task UpdateDoctorById_SingleFieldProvided_UpdatesSpecifiedField(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Doctors.Add(doctorToUpdate);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest();
        typeof(UpdateDoctorRequest).GetProperty(fieldName)!.SetValue(updateDoctorRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        var updatedDoctor = DbContext.Doctors.First(d => d.AppUser.Id == doctorId);
        updatedDoctor.Should().NotBeNull();

        switch (fieldName)
        {
            case "UserName":
                updatedDoctor.AppUser.UserName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedDoctor.AppUser.Email.Should().Be(fieldValue);
                break;
            case "PhoneNumber":
                updatedDoctor.AppUser.PhoneNumber.Should().Be(fieldValue);
                break;
            case "NewPassword":
                updatedDoctor.AppUser.PasswordHash.Should().NotBe(oldPasswordHash);
                break;
            default:
                throw new Exception("No assertion for field name: " + fieldName);
        }
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("UserName", "a")]
    [InlineData("Email", "")]
    [InlineData("Email", "a")]
    [InlineData("PhoneNumber", "")]
    [InlineData("NewPassword", "")]
    public async Task UpdateDoctorById_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Doctors.Add(doctorToUpdate);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest();
        typeof(UpdateDoctorRequest).GetProperty(fieldName)!.SetValue(updateDoctorRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDoctorById_UserNameAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Doctors.Add(doctorToUpdate);

        var conflictingDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "confUserName",
                NormalizedUserName = "confUserName".ToUpper()
            }
        };
        DbContext.Doctors.Add(conflictingDoctor);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = conflictingDoctor.AppUser.UserName
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateDoctorById_EmailAlreadyExists_ReturnsConflict()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        const string oldDoctorPassword = "oldPassword1234#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, oldDoctorPassword);
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };
        DbContext.Doctors.Add(doctorToUpdate);

        var conflictingDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "conflictingMail@mail.com",
                NormalizedEmail = "conflictingMail@mail.com".ToUpper()
            }
        };
        DbContext.Doctors.Add(conflictingDoctor);

        await DbContext.SaveChangesAsync();

        var updateDoctorRequest = new UpdateDoctorRequest
        {
            Email = conflictingDoctor.AppUser.Email
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateDoctorById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var doctorId = Guid.NewGuid();
        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = "newUserName"
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Doctor)]
    [InlineData(RoleTypes.Patient)]
    public async Task UpdateDoctorById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var doctorId = Guid.NewGuid();
        var updateDoctorRequest = new UpdateDoctorRequest
        {
            UserName = "newUserName"
        };

        var serializedContent = JsonConvert.SerializeObject(updateDoctorRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{doctorId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorExists_DeletesDoctorWithSpecifiedId()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var doctorId = Guid.NewGuid();
        var doctorToDelete = new Doctor { AppUser = new AppUser { Id = doctorId } };
        DbContext.Doctors.Add(doctorToDelete);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();

        DbContext.Doctors.Should().NotContain(d => d.Id == doctorId);
    }

    [Fact]
    public async Task DeleteDoctorById_UserWithSpecifiedIdIsNotDoctor_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var notDoctorId = Guid.NewGuid();
        var notDoctor = new Patient
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser { Id = notDoctorId }
        };
        DbContext.Patients.Add(notDoctor);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/{notDoctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDoctorById_UserWithSpecifiedIdDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var doctorId = "1";

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDoctorById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var doctorId = Guid.NewGuid();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Doctor)]
    [InlineData(RoleTypes.Patient)]
    public async Task DeleteDoctorById_AuthenticatedUserIsNotAdmin_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var doctorId = Guid.NewGuid();

        // act
        var response = await client.DeleteAsync($"{UrlPrefix}/{doctorId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Doctor> CreateDoctorAsync(CreateDoctorCommand createPatientCommand)
    {
        var hasher = new PasswordHasher<AppUser>();

        var newDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                UserName = createPatientCommand.UserName,
                NormalizedUserName = createPatientCommand.UserName.ToUpper(),
                Email = createPatientCommand.Email,
                NormalizedEmail = createPatientCommand.Email.ToUpper(),
                PhoneNumber = createPatientCommand.PhoneNumber,
                PasswordHash = hasher.HashPassword(null!, createPatientCommand.Password),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        };
        DbContext.Doctors.Add(newDoctor);
        var doctorRoleId = DbContext.Roles.FirstOrDefault(r => r.Name == RoleTypes.Doctor)!.Id;
        DbContext.IdentityUserRole.Add(new IdentityUserRole<Guid>
        {
            UserId = newDoctor.AppUser.Id,
            RoleId = doctorRoleId,
        });
        await DbContext.SaveChangesAsync();

        return newDoctor;
    }
}