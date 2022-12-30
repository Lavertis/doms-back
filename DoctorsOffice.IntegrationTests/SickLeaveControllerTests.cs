using System.Net;
using System.Text;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Wrappers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class SickLeaveControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/sick-leaves";

    public SickLeaveControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_ValidPatientId_ReturnsSickLeavesByPatientId()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        var patient = new Patient
        {
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = patientId, FirstName = "", LastName = ""}
        };
        DbContext.Patients.Add(patient);

        var otherPatientId = Guid.NewGuid();
        var otherPatient = new Patient
        {
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = otherPatientId, FirstName = "", LastName = "",}
        };
        DbContext.Patients.Add(otherPatient);


        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = authenticatedDoctorId,
                PatientId = i % 2 == 0 ? patientId : otherPatientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();
        var expectedSickLeaves = sickLeaves
            .Where(p => p.PatientId == patientId)
            .Select(p => Mapper.Map<SickLeaveResponse>(p));

        foreach (var expectedSickLeave in expectedSickLeaves)
            responseContent.Records.Should().ContainSingle(s => s.Id == expectedSickLeave.Id);
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_PatientIdDoesntMatchAnyPatient_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var patientId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_PatientDoesntHaveAnySickLeaves_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        var patient = new Patient
        {
            Address = "",
            AppUser = new AppUser {Id = patientId, FirstName = "", LastName = "",}
        };
        DbContext.Patients.Add(patient);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var patientId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task GetSickLeavesByPatientId_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{Guid.NewGuid()}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_NoPaginationProvided_ReturnsSickLeavesByPatientId()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        var patient = new Patient
        {
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = patientId, FirstName = "", LastName = ""}
        };
        DbContext.Patients.Add(patient);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = authenticatedDoctorId,
                PatientId = patientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent =
            sickLeaves.Select(s => Mapper.Map<SickLeaveResponse>(s));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task GetSickLeavesByPatientId_PaginationProvided_ReturnsRequestedPage()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        const int pageSize = 3;
        const int pageNumber = 2;

        var patient = new Patient
        {
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = patientId, FirstName = "", LastName = ""}
        };
        DbContext.Patients.Add(patient);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = authenticatedDoctorId,
                PatientId = patientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = sickLeaves
            .Select(sickLeave => Mapper.Map<SickLeaveResponse>(sickLeave))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);


        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedPatient_AuthenticatedUserIsPatient_ReturnsSickLeavesForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var otherPatientId = Guid.NewGuid();
        var otherPatient = new Patient
        {
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = otherPatientId, FirstName = "", LastName = "",}
        };
        DbContext.Patients.Add(otherPatient);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = i % 2 == 0 ? authenticatedPatientId : otherPatientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        var expectedSickLeaves = sickLeaves
            .Where(p => p.PatientId == authenticatedPatientId)
            .Select(s => Mapper.Map<SickLeaveResponse>(s));

        foreach (var expectedSickLeave in expectedSickLeaves)
            responseContent.Records.Should().ContainSingle(p => p.Id == expectedSickLeave.Id);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedPatient_AuthenticatedPatientDoesntHaveSickLeaves_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSickLeavesForAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Doctor)]
    public async Task GetSickLeavesForAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(
        string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedPatient_NoPaginationProvided_ReturnsSickLeavesForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = authenticatedPatientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = sickLeaves
            .Select(p => Mapper.Map<SickLeaveResponse>(p));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedPatient_PaginationProvided_ReturnsSickLeavesForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = authenticatedPatientId,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = sickLeaves
            .Select(p => Mapper.Map<SickLeaveResponse>(p))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedDoctor_AuthenticatedUserIsDoctor_ReturnsSickLeavesForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var otherDoctorId = Guid.NewGuid();
        var otherDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                Id = otherDoctorId, FirstName = "oldFirstName", LastName = "oldLastName"
            }
        };
        DbContext.Doctors.Add(otherDoctor);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = i % 2 == 0 ? authenticatedDoctorId : otherDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();
        var expectedSickLeaves = sickLeaves
            .Where(p => p.DoctorId == authenticatedDoctorId)
            .Select(p => Mapper.Map<SickLeaveResponse>(p));

        foreach (var expectedSickLeave in expectedSickLeaves)
            responseContent.Records.Should().ContainSingle(p => p.Id == expectedSickLeave.Id);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedDoctor_AuthenticatedDoctorDoesntHaveSickLeaves_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSickLeavesForAuthenticatedDoctor_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task GetSickLeavesForAuthenticatedDoctor_AuthenticatedUserIsNotDoctor_ReturnsForbidden(
        string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedDoctor_NoPaginationProvided_ReturnsSickLeavesForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = authenticatedDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedSickLeaves = sickLeaves
            .Select(p => Mapper.Map<SickLeaveResponse>(p));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedSickLeaves);
    }

    [Fact]
    public async Task
        GetSickLeavesForAuthenticatedDoctor_PaginationProvided_ReturnsSickLeavesForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 10; i++)
        {
            sickLeaves.Add(new SickLeave()
            {
                DoctorId = authenticatedDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DateStart = DateTime.UtcNow.AddDays(i),
                DateEnd = DateTime.UtcNow.AddDays(i + 4),
                Diagnosis = "Diagnosis",
                Purpose = "Purpose"
            });
        }

        DbContext.SickLeaves.AddRange(sickLeaves);
        await DbContext.SaveChangesAsync();

        var expectedSickLeaves = sickLeaves
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => Mapper.Map<SickLeaveResponse>(p));

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<SickLeaveResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedSickLeaves);
    }

    [Fact]
    public async Task CreateSickLeve_ValidRequest_CreatesSickLeaves()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var createSickLeaveRequest = new CreateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(4),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createSickLeaveRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<SickLeaveResponse>();
        var createdSickLeave = DbContext.SickLeaves
            .First(s => s.Id == responseContent.Id);

        createdSickLeave.PatientId.Should().Be(createSickLeaveRequest.PatientId);
        createdSickLeave.Diagnosis.Should().Be(createSickLeaveRequest.Diagnosis);
        createdSickLeave.Purpose.Should().Be(createdSickLeave.Purpose);
        createdSickLeave.DateStart.Should().Be(createdSickLeave.DateStart);
        createdSickLeave.DateEnd.Should().Be(createdSickLeave.DateEnd);
        createdSickLeave.AppointmentId.Should().Be(createdSickLeave.AppointmentId);
        createdSickLeave.DoctorId.Should().Be(authenticatedDoctorId);
    }

    [Theory]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    [InlineData("DateStart", null)]
    [InlineData("DateEnd", null)]
    public async Task CreateSickLeave_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var createSickLeaveRequest = new CreateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow,
            DateEnd = DateTime.UtcNow.AddDays(4),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldValue, out var id);
            typeof(CreateSickLeaveRequest)
                .GetProperty(fieldName)!
                .SetValue(createSickLeaveRequest, id);
        }
        else
        {
            typeof(CreateSickLeaveRequest)
                .GetProperty(fieldName)!
                .SetValue(createSickLeaveRequest, fieldValue);
        }

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createSickLeaveRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSickLeave_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createSickLeaveRequest = new CreateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow,
            DateEnd = DateTime.UtcNow.AddDays(4),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createSickLeaveRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task CreateSickLeave_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var createSickLeaveRequest = new CreateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(4),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createSickLeaveRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateSickLeaveById_ValidRequest_UpdatesSickLeave()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var sickLeaveToUpdate = new SickLeave()
        {
            Id = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
            PatientId = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(5),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        DbContext.SickLeaves.Add(sickLeaveToUpdate);
        await DbContext.SaveChangesAsync();

        var updateSickLeaveRequest = new UpdateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(4),
            AppointmentId = DbContext.Appointments.First().Id,
            Diagnosis = "Diagnosis2",
            Purpose = "Purpose3"
        };

        var serializedContent = JsonConvert.SerializeObject(updateSickLeaveRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{sickLeaveToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedSickLeave = DbContext.SickLeaves
            .First(s => s.Id == sickLeaveToUpdate.Id);

        updatedSickLeave.PatientId.Should().Be(updateSickLeaveRequest.PatientId.Value);
        updatedSickLeave.DoctorId.Should().Be(sickLeaveToUpdate.DoctorId);
        updatedSickLeave.AppointmentId.Should().Be(updateSickLeaveRequest.AppointmentId);
        updatedSickLeave.Diagnosis.Should().Be(updateSickLeaveRequest.Diagnosis);
        updatedSickLeave.Purpose.Should().Be(updateSickLeaveRequest.Purpose);
        updatedSickLeave.DateStart.Should().Be(updateSickLeaveRequest.DateStart);
    }

    [Theory]
    [InlineData("Diagnosis", "Diagnosis sclerosis")]
    [InlineData("Purpose", "NO bo tak")]
    public async Task UpdateSickLeaveById_SingleFieldProvided_UpdatesProvidedField(string fieldName,
        string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var sickLeaveToUpdate = new SickLeave()
        {
            Id = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
            PatientId = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(5),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        DbContext.SickLeaves.Add(sickLeaveToUpdate);
        await DbContext.SaveChangesAsync();

        var updateSickLeaveRequest = new UpdateSickLeaveRequest();

        typeof(UpdateSickLeaveRequest)
            .GetProperty(fieldName)!
            .SetValue(updateSickLeaveRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateSickLeaveRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{sickLeaveToUpdate.Id}", content);
        // var stuff = response.Content.ReadAsStringAsync().Result;

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedSickLeave = DbContext.SickLeaves.First(s => s.Id == sickLeaveToUpdate.Id);
        typeof(SickLeave).GetProperty(fieldName)!.GetValue(updatedSickLeave).Should().Be(
            typeof(UpdateSickLeaveRequest).GetProperty(fieldName)!.GetValue(updateSickLeaveRequest)
        );
    }

    [Theory]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    public async Task UpdateSickLeavesById_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var sickLeaveToUpdate = new SickLeave()
        {
            Id = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
            PatientId = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.AddDays(5),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        DbContext.SickLeaves.Add(sickLeaveToUpdate);
        await DbContext.SaveChangesAsync();

        var updateSickLeaveRequest = new UpdateSickLeaveRequest();

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldValue, out var id);
            typeof(UpdateSickLeaveRequest)
                .GetProperty(fieldName)!
                .SetValue(updateSickLeaveRequest, id);
        }
        else
        {
            typeof(UpdateSickLeaveRequest)
                .GetProperty(fieldName)!
                .SetValue(updateSickLeaveRequest, fieldValue);
        }

        var serializedContent = JsonConvert.SerializeObject(updateSickLeaveRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{sickLeaveToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSickLeaveById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var updateSickLeaveRequest = new UpdateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow,
            DateEnd = DateTime.UtcNow.AddDays(4),
            AppointmentId = DbContext.Appointments.First().Id,
            Diagnosis = "Diagnosis2",
            Purpose = "Purpose3"
        };

        var serializedContent = JsonConvert.SerializeObject(updateSickLeaveRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task UpdateSickLeaveById_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var updateSickLeaveRequest = new UpdateSickLeaveRequest()
        {
            PatientId = DbContext.Patients.First().Id,
            DateStart = DateTime.UtcNow,
            DateEnd = DateTime.UtcNow.AddDays(4),
            AppointmentId = DbContext.Appointments.First().Id,
            Diagnosis = "Diagnosis2",
            Purpose = "Purpose3"
        };

        var serializedContent = JsonConvert.SerializeObject(updateSickLeaveRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}