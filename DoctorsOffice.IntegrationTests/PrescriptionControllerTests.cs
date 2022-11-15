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

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = authenticatedDoctorId,
                PatientId = i % 2 == 0 ? patientId : otherPatientId,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();
        var expectedPrescriptions = prescriptions
            .Where(p => p.PatientId == patientId)
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Records.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
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
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEmpty();
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
            Address = "",
            AppUser = new AppUser {Id = patientId, FirstName = "", LastName = "",}
        };
        DbContext.Patients.Add(patient);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEmpty();
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
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
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
    public async Task GetPrescriptionsByPatientId_NoPaginationProvided_ReturnsPrescriptionByPatientId()
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

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = authenticatedDoctorId,
                PatientId = patientId,
                DrugItems = new List<DrugItem> {new() {Dosage = "1-1-1", Name = "Drug1"}}
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent =
            prescriptions.Select(prescription => Mapper.Map<PrescriptionResponse>(prescription));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task GetPrescriptionsByPatientId_PaginationProvided_ReturnsRequestedPage()
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

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = authenticatedDoctorId,
                PatientId = patientId,
                DrugItems = new List<DrugItem> {new() {Dosage = "1-1-1", Name = "Drug1"}}
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = prescriptions
            .Select(prescription => Mapper.Map<PrescriptionResponse>(prescription))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/{patientId}?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
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
            Address = "",
            NationalId = "",
            AppUser = new AppUser {Id = otherPatientId, FirstName = "", LastName = "",}
        };
        DbContext.Patients.Add(otherPatient);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = i % 2 == 0 ? authenticatedPatientId : otherPatientId,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        var expectedPrescriptions = prescriptions
            .Where(p => p.PatientId == authenticatedPatientId)
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Records.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
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
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEmpty();
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
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Doctor)]
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
        GetPrescriptionsForAuthenticatedPatient_NoPaginationProvided_ReturnsPrescriptionsForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = authenticatedPatientId,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = prescriptions
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
    }

    [Fact]
    public async Task
        GetPrescriptionsForAuthenticatedPatient_PaginationProvided_ReturnsPrescriptionsForAuthenticatedPatient()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedPatientId = await AuthenticateAsPatientAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = DbContext.Doctors.First().Id,
                PatientId = authenticatedPatientId,
                DrugItems = new List<DrugItem>
                {
                    new() {Id = Guid.NewGuid(), Dosage = "1-1-1", Name = "Drug1"},
                    new() {Id = Guid.NewGuid(), Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = prescriptions
            .Select(p => Mapper.Map<PrescriptionResponse>(p))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
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
            AppUser = new AppUser
            {
                Id = otherDoctorId, FirstName = "oldFirstName", LastName = "oldLastName"
            }
        };
        DbContext.Doctors.Add(otherDoctor);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = i % 2 == 0 ? authenticatedDoctorId : otherDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();
        var expectedPrescriptions = prescriptions
            .Where(p => p.DoctorId == authenticatedDoctorId)
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        foreach (var expectedPrescription in expectedPrescriptions)
            responseContent.Records.Should().ContainSingle(p => p.Id == expectedPrescription.Id);
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
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
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
    public async Task
        GetPrescriptionsForAuthenticatedDoctor_NoPaginationProvided_ReturnsPrescriptionsForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = authenticatedDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedPrescriptions = prescriptions
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedPrescriptions);
    }

    [Fact]
    public async Task
        GetPrescriptionsForAuthenticatedDoctor_PaginationProvided_ReturnsPrescriptionsForAuthenticatedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        const int pageSize = 3;
        const int pageNumber = 2;

        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 10; i++)
        {
            prescriptions.Add(new Prescription
            {
                DoctorId = authenticatedDoctorId,
                PatientId = DbContext.Patients.First().Id,
                DrugItems = new List<DrugItem>
                {
                    new() {Dosage = "1-1-1", Name = "Drug1"},
                    new() {Dosage = "2-2-2", Name = "Drug2"}
                }
            });
        }

        DbContext.Prescriptions.AddRange(prescriptions);
        await DbContext.SaveChangesAsync();

        var expectedPrescriptions = prescriptions
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => Mapper.Map<PrescriptionResponse>(p));

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<PrescriptionResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedPrescriptions);
    }

    [Fact]
    public async Task CreatePrescription_ValidRequest_CreatesPrescription()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            FulfillmentDeadline = DateTime.UtcNow.AddDays(1),
            DrugItems = new List<CreateDrugItemRequest>
            {
                new() {Dosage = "1-1-1", Name = "Drug1"},
                new() {Dosage = "2-2-2", Name = "Drug2"}
            }
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

        createdPrescription.PatientId.Should().Be(createPrescriptionRequest.PatientId);
        createdPrescription.FulfillmentDeadline.Should().Be(createPrescriptionRequest.FulfillmentDeadline);
        createdPrescription.DrugItems.Should()
            .Contain(d => d.Rxcui == createPrescriptionRequest.DrugItems.First().Rxcui);
        createdPrescription.DoctorId.Should().Be(authenticatedDoctorId);
    }

    [Theory]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    public async Task CreatePrescription_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest>
            {
                new() {Dosage = "1-1-1", Name = "Drug1"},
                new() {Dosage = "2-2-2", Name = "Drug2"}
            }
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
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest>()
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
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest> {new() {Rxcui = 1}, new() {Rxcui = 2}}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task CreatePrescription_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var createPrescriptionRequest = new CreatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest> {new() {Rxcui = 1}, new() {Rxcui = 2}}
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createPrescriptionRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePrescriptionById_ValidRequest_UpdatesPrescription()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
            DrugItems = new List<DrugItem>
            {
                new() {Dosage = "1-1-1", Name = "Drug1"},
                new() {Dosage = "2-2-2", Name = "Drug2"}
            }
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest>
            {
                new() {Rxcui = 1, Dosage = "1-1-1", Name = "Drug1"},
                new() {Rxcui = 2, Dosage = "2-2-2", Name = "Drug2"}
            }
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

        updatedPrescription.PatientId.Should().Be(updatePrescriptionRequest.PatientId.Value);
        foreach (var drugItem in updatePrescriptionRequest.DrugItems)
            updatedPrescription.DrugItems.Should().Contain(d => d.Rxcui == drugItem.Rxcui);
    }

    [Theory]
    [InlineData("FulfillmentDeadline", "3022-10-08T14:09:39.309Z")]
    public async Task UpdatePrescriptionById_SingleFieldProvided_UpdatesProvidedField(string fieldName,
        string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest();

        if (DateTime.TryParse(fieldValue, out var date))
            typeof(UpdatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(updatePrescriptionRequest, date);
        else
            typeof(UpdatePrescriptionRequest)
                .GetProperty(fieldName)!
                .SetValue(updatePrescriptionRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);
        var stuff = response.Content.ReadAsStringAsync().Result;

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var updatedPrescription = DbContext.Prescriptions.First(p => p.Id == prescriptionToUpdate.Id);

        if (updatePrescriptionRequest.FulfillmentDeadline is not null)
            updatedPrescription.FulfillmentDeadline.Should().Be(updatePrescriptionRequest.FulfillmentDeadline);
        if (updatePrescriptionRequest.PatientId is not null)
            updatedPrescription.PatientId.Should().Be(updatePrescriptionRequest.PatientId.Value);
    }

    [Theory]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "invalidGuid")]
    public async Task UpdatePrescriptionById_SingleFieldInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
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
    public async Task UpdatePrescriptionById_DrugListNotNullAndEmpty_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedDoctorId = await AuthenticateAsDoctorAsync(client);

        var prescriptionToUpdate = new Prescription
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DoctorId = authenticatedDoctorId,
        };
        DbContext.Prescriptions.Add(prescriptionToUpdate);
        await DbContext.SaveChangesAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            DrugItems = Enumerable.Empty<CreateDrugItemRequest>().ToList()
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{prescriptionToUpdate.Id}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePrescriptionById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest> {new() {Rxcui = 1}, new() {Rxcui = 2}}
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Patient)]
    public async Task UpdatePrescriptionById_AuthenticatedUserIsNotDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        var updatePrescriptionRequest = new UpdatePrescriptionRequest
        {
            PatientId = DbContext.Patients.First().Id,
            DrugItems = new List<CreateDrugItemRequest> {new() {Rxcui = 1}, new() {Rxcui = 2}}
        };

        var serializedContent = JsonConvert.SerializeObject(updatePrescriptionRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{Guid.NewGuid()}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}