using System.Net;
using System.Text;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class PrescriptionControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/prescriptions";

    public PrescriptionControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPrescriptionsByPatientId_ValidPatientId_ReturnsPrescriptionByPatientId()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        var patient = new Patient
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Id = patientId}
        };
        DbContext.Patients.Add(patient);

        var otherPatientId = Guid.NewGuid();
        var otherPatient = new Patient
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Id = otherPatientId}
        };
        DbContext.Patients.Add(otherPatient);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                Title = "",
                Description = "",
                DoctorId = authenticatedDoctorId,
                PatientId = i % 2 == 0 ? patientId : otherPatientId,
                DrugItems = new List<DrugItem> {new()}
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();
        var expectedPrescriptions = prescriptions
            .Where(p => p.PatientId == patientId)
            .Select(p => new PrescriptionResponse(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
    }

    [Fact]
    public async Task GetPrescriptionsByPatientId_PatientIdDoesntMatchAnyPatient_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var patientId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsByPatientId_PatientDoesntHaveAnyPrescriptions_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);
        var patientId = Guid.NewGuid();

        var patient = new Patient
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Id = patientId}
        };
        DbContext.Patients.Add(patient);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsByPatientId_NoAuthenticatedUser_ReturnsUnauthorized()
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
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Patient)]
    public async Task GetPrescriptionsByPatientId_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
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
    public async Task
        GetPrescriptionsForAuthenticatedPatient_AuthenticatedUserIsPatient_ReturnsPrescriptionsForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var otherPatientId = Guid.NewGuid();
        var otherPatient = new Patient
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Id = otherPatientId}
        };
        DbContext.Patients.Add(otherPatient);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                Title = "",
                Description = "",
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = i % 2 == 0 ? authenticatedPatientId : otherPatientId,
                DrugItems = new List<DrugItem> {new(), new()}
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();

        var expectedPrescriptions = prescriptions
            .Where(p => p.PatientId == authenticatedPatientId)
            .Select(p => new PrescriptionResponse(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
    }


    [Fact]
    public async Task
        GetPrescriptionsForAuthenticatedPatient_AuthenticatedPatientDoesntHavePrescriptions_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();

        responseContent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsForAuthenticatedPatient_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Doctor)]
    public async Task GetPrescriptionsForAuthenticatedPatient_AuthenticatedUserIsNotPatient_ReturnsForbidden(
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
        GetPrescriptionsForAuthenticatedDoctor_AuthenticatedUserIsDoctor_ReturnsPrescriptionsForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var otherDoctorId = Guid.NewGuid();
        var otherDoctor = new Doctor
        {
            AppUser = new AppUser {Id = otherDoctorId}
        };
        DbContext.Doctors.Add(otherDoctor);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                Title = "",
                Description = "",
                DoctorId = i % 2 == 0 ? authenticatedDoctorId : otherDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DrugItems = new List<DrugItem> {new(), new()}
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<List<PrescriptionResponse>>();
        var expectedPrescriptions = prescriptions
            .Where(p => p.DoctorId == authenticatedDoctorId)
            .Select(p => new PrescriptionResponse(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
    }

    [Fact]
    public async Task
        GetPrescriptionsForAuthenticatedDoctor_AuthenticatedDoctorDoesntHavePrescriptions_ReturnsEmptyList()
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
    public async Task GetPrescriptionsForAuthenticatedDoctor_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Patient)]
    public async Task GetPrescriptionsForAuthenticatedDoctor_AuthenticatedUserIsNotDoctor_ReturnsForbidden(
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
    public async Task CreatePrescription_ValidRequest_CreatesPrescription()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            Title = "Title",
            Description = "Description",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PrescriptionResponse>();
        var createdPrescription = DbContext.Prescriptions
            .Include(p => p.DrugItems)
            .First(p => p.Id == responseContent.Id);

        createdPrescription.Title.Should().Be(createPrescriptionRequest.Title);
        createdPrescription.Description.Should().Be(createPrescriptionRequest.Description);
        createdPrescription.PatientId.Should().Be(createPrescriptionRequest.PatientId);
        createdPrescription.DrugItems.Should().Contain(d => d.Id == createPrescriptionRequest.DrugsIds.First());
        createdPrescription.DoctorId.Should().Be(authenticatedDoctorId);
    }

    [Theory]
    [InlineData("Title", "")]
    [InlineData("Description", "")]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    public async Task CreatePrescription_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            Title = "Title",
            Description = "Description",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldValue, out var id);
            typeof(CreatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(createPrescriptionRequest, id);
        }
        else
        {
            typeof(CreatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(createPrescriptionRequest, fieldValue);
        }

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePrescription_DrugListIsEmpty_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            Title = "Title",
            Description = "Description",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid>()
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePrescription_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            Title = "Title",
            Description = "Description",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Patient)]
    public async Task CreatePrescription_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            Title = "Title",
            Description = "Description",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePrescription_ValidRequest_UpdatesPrescription()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            Title = "OldTitle",
            Description = "OldDescription",
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
            DrugItems = new List<DrugItem> {new() {Id = Guid.NewGuid()}}
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            Title = "NewTitle",
            Description = "NewDescription",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedPrescription = DbContext.Prescriptions
            .Include(p => p.DrugItems)
            .First(p => p.Id == prescriptionToUpdate.Id);

        updatedPrescription.Title.Should().Be(updatePrescriptionRequest.Title);
        updatedPrescription.Description.Should().Be(updatePrescriptionRequest.Description);
        updatedPrescription.PatientId.Should().Be(updatePrescriptionRequest.PatientId.Value);
        foreach (var drugId in updatePrescriptionRequest.DrugsIds)
            updatedPrescription.DrugItems.Should().Contain(d => d.Id == drugId);
    }

    [Theory]
    [InlineData("Title", "NewTitle")]
    [InlineData("Description", "NewDescription")]
    public async Task UpdatePrescription_SingleFieldProvided_UpdatesProvidedField(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            Title = "OldTitle",
            Description = "OldDescription",
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest();

        typeof(UpdatePrescriptionRequest).GetProperty(fieldName)!.SetValue(updatePrescriptionRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedPrescription = DbContext.Prescriptions.First(p => p.Id == prescriptionToUpdate.Id);

        if (updatePrescriptionRequest.Title is not null)
            updatedPrescription.Title.Should().Be(updatePrescriptionRequest.Title);
        if (updatePrescriptionRequest.Description is not null)
            updatedPrescription.Description.Should().Be(updatePrescriptionRequest.Description);
        if (updatePrescriptionRequest.PatientId is not null)
            updatedPrescription.PatientId.Should().Be(updatePrescriptionRequest.PatientId.Value);
    }

    [Theory]
    [InlineData("Title", "")]
    [InlineData("Description", "")]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    public async Task UpdatePrescription_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            Title = "OldTitle",
            Description = "OldDescription",
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest();

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldValue, out var id);
            typeof(UpdatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(updatePrescriptionRequest, id);
        }
        else
        {
            typeof(UpdatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(updatePrescriptionRequest, fieldValue);
        }

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePrescription_DrugListNotNullAndEmpty_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            Title = "OldTitle",
            Description = "OldDescription",
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            DrugsIds = Enumerable.Empty<Guid>().ToList()
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePrescription_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            Title = "NewTitle",
            Description = "NewDescription",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(RoleTypes.Admin)]
    [InlineData(RoleTypes.Patient)]
    public async Task UpdatePrescription_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            Title = "NewTitle",
            Description = "NewDescription",
            PatientId = DbContext.Patients.First().Id,
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}