﻿using System.Net;
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

public class AppointmentControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/appointments";

    public AppointmentControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_UserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_UserDoesntHaveAppointments_ReturnsEmptyList()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentResponse>>();

        responseContent.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_UserHasAppointments_ReturnsOnlyUserAppointments()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var userId = await AuthenticateAsPatientAsync(client);
        var user = (await DbContext.Patients.FindAsync(userId))!;
        var doctor = DbContext.Doctors.First();
        var appointmentStatus = DbContext.AppointmentStatuses.First();
        var appointmentType = DbContext.AppointmentTypes.First();
        var otherPatient = new Patient
        {
            AppUser = new AppUser {FirstName = "", LastName = ""},
            Address = "",
            NationalId = ""
        };

        const int appointmentsCount = 3;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentsCount; i++)
        {
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = doctor,
                Patient = user,
                Status = appointmentStatus,
                Type = appointmentType
            });
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = doctor,
                Patient = otherPatient,
                Status = appointmentStatus,
                Type = appointmentType
            });
        }

        DbContext.Appointments.AddRange(appointments);
        DbContext.Patients.Add(otherPatient);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentResponse>>();
        responseContent.Records.Should().HaveCount(appointmentsCount);
        DbContext.Appointments.Where(a => a.Patient.Id == userId).ToList()
            .Should().OnlyContain(
                userAppointment =>
                    responseContent.Records.Any(responseAppointment => responseAppointment.Id == userAppointment.Id)
            );
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_AuthenticatedUserIsDoctor_ReturnsOnlyDoctorAppointments()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var authenticatedUser = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.First();
        var appointmentStatus = DbContext.AppointmentStatuses.First();
        var appointmentType = DbContext.AppointmentTypes.First();
        var otherDoctor = new Doctor
        {
            AppUser = new AppUser {FirstName = "", LastName = ""}
        };

        const int appointmentsCount = 3;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentsCount; i++)
        {
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = authenticatedUser,
                Patient = patient,
                Status = appointmentStatus,
                Type = appointmentType
            });
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = otherDoctor,
                Patient = patient,
                Status = appointmentStatus,
                Type = appointmentType
            });
        }

        DbContext.Appointments.AddRange(appointments);
        DbContext.Doctors.Add(otherDoctor);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentResponse>>();

        responseContent.Records.Should().HaveCount(appointmentsCount);
        DbContext.Appointments.Where(a => a.Doctor.Id == authenticatedUserId).ToList()
            .Should().OnlyContain(
                userAppointment =>
                    responseContent.Records.Any(responseAppointment => responseAppointment.Id == userAppointment.Id)
            );
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_AuthenticatedUserIsAdmin_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_NoPaginationProvided_ReturnsOnlyUserAppointments()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var userId = await AuthenticateAsPatientAsync(client);
        var user = (await DbContext.Patients.FindAsync(userId))!;
        var doctor = DbContext.Doctors.First();
        var appointmentStatus = DbContext.AppointmentStatuses.First();
        var appointmentType = DbContext.AppointmentTypes.First();

        const int appointmentsCount = 3;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentsCount; i++)
        {
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = doctor,
                Patient = user,
                Status = appointmentStatus,
                Type = appointmentType
            });
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = appointments.Select(appointment => Mapper.Map<AppointmentResponse>(appointment));

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentResponse>>();
        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
    }

    [Fact]
    public async Task GetAllAppointmentsForAuthenticatedUser_PaginationProvided_ReturnsRequestedPage()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var userId = await AuthenticateAsPatientAsync(client);
        var user = (await DbContext.Patients.FindAsync(userId))!;
        var doctor = DbContext.Doctors.First();
        var appointmentStatus = DbContext.AppointmentStatuses.First();
        var appointmentType = DbContext.AppointmentTypes.First();

        const int pageSize = 3;
        const int pageNumber = 2;
        const int appointmentsCount = 20;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentsCount; i++)
        {
            appointments.Add(new Appointment
            {
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Description",
                Doctor = doctor,
                Patient = user,
                Status = appointmentStatus,
                Type = appointmentType
            });
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var expectedResponseContent = appointments
            .Select(appointment => Mapper.Map<AppointmentResponse>(appointment))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentResponse>>();
        responseContent.Records.Should().BeEquivalentTo(expectedResponseContent);
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
    }

    [Fact]
    public async Task GetAppointmentById_UserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var appointmentId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointmentId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAppointmentById_AppointmentDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        const long appointmentId = 999;

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointmentId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppointmentById_AppointmentDoesntBelongToAuthenticatedPatient_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        await DbContext.Patients.FindAsync(authenticatedUserId);
        var otherPatient = new Patient
        {
            AppUser = new AppUser {FirstName = "", LastName = ""},
            Address = "",
            NationalId = ""
        };
        var doctor = DbContext.Doctors.First();
        var appointment = new Appointment
        {
            Date = DateTime.UtcNow,
            Description = "Description",
            Doctor = doctor,
            Patient = otherPatient,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Patients.Add(otherPatient);
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointment.Id}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentById_AppointmentDoesntBelongToAuthenticatedDoctor_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);
        var otherDoctor = new Doctor
        {
            AppUser = new AppUser
            {
                FirstName = "FirstName",
                LastName = "LastName"
            }
        };
        var patient = DbContext.Patients.First();
        var appointment = new Appointment
        {
            Date = DateTime.UtcNow,
            Description = "Description",
            Doctor = otherDoctor,
            Patient = patient,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Doctors.Add(otherDoctor);
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointment.Id}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentById_AuthenticatedUserIsAdmin_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var appointmentId = Guid.NewGuid();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointmentId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentById_RequestedAppointmentBelongsToAuthenticatedPatient_ReturnsAppointment()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var authenticatedUser = (await DbContext.Patients.FindAsync(authenticatedUserId))!;
        var doctor = DbContext.Doctors.First();
        var appointment = new Appointment
        {
            Date = DateTime.UtcNow,
            Description = "Description",
            Doctor = doctor,
            Patient = authenticatedUser,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var expectedResponse = Mapper.Map<AppointmentResponse>(appointment);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointment.Id}");

        // assert
        var responseContent = await response.Content.ReadAsAsync<AppointmentResponse>();
        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentById_RequestedAppointmentBelongsToAuthenticatedDoctor_ReturnsAppointment()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var authenticatedUser = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.First();
        var appointment = new Appointment
        {
            Date = DateTime.UtcNow,
            Description = "Description",
            Doctor = authenticatedUser,
            Patient = patient,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var expectedResponse = Mapper.Map<AppointmentResponse>(appointment);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/user/current/{appointment.Id}");

        // assert
        var responseContent = await response.Content.ReadAsAsync<AppointmentResponse>();
        responseContent.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData("dateStart", "2022-07-04T15:12:52Z")]
    [InlineData("dateEnd", "2022-07-04T15:12:52Z")]
    [InlineData("patientId", "7945992e-3b96-4f0b-9143-f8db38cd8b5e")]
    [InlineData("typeId", "e58cabc9-e259-42ff-a2a1-0e8d39bb900e")]
    [InlineData("statusId", "8445a2f4-97cd-45c9-921f-f649f85cc0be")]
    public async Task GetAppointmentsFiltered_SingleFilterProvided_ReturnsAppointmentsMatchingProvidedFilter(
        string filterName, string filterValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);

        var doctor1 = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var doctor2 = new Doctor {AppUser = new AppUser {FirstName = "", LastName = ""}};
        DbContext.Doctors.Add(doctor2);
        var doctors = new List<Doctor> {doctor1, doctor2};

        var patient1 = DbContext.Patients.First();
        var patient2 = new Patient
        {
            AppUser = new AppUser
            {
                Id = Guid.Parse("7945992e-3b96-4f0b-9143-f8db38cd8b5e"),
                FirstName = "", LastName = ""
            },
            Address = "",
            NationalId = ""
        };
        DbContext.Patients.Add(patient2);
        var patients = new List<Patient> {patient1, patient2};

        var appointmentTypes = DbContext.AppointmentTypes.ToList();
        var appointmentStatuses = DbContext.AppointmentStatuses.ToList();
        const int appointmentCount = 10;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount / 2; i++)
        {
            var appointment = new Appointment
            {
                Date = DateTime.Parse("2022-07-01T15:12:52Z").AddDays(i + 1),
                Description = "Description",
                Doctor = doctors[i % doctors.Count],
                Patient = patients[0],
                Status = appointmentStatuses[i % appointmentStatuses.Count],
                Type = appointmentTypes[i % appointmentTypes.Count]
            };
            appointments.Add(appointment);
            appointment = new Appointment
            {
                Date = DateTime.Parse("2022-07-01T15:12:52Z").AddDays(i + 1),
                Description = "Description",
                Doctor = doctors[i % doctors.Count],
                Patient = patients[1],
                Status = appointmentStatuses[i % appointmentStatuses.Count],
                Type = appointmentTypes[i % appointmentTypes.Count]
            };
            appointments.Add(appointment);
        }

        var doctorAppointments = appointments.Where(a => a.Doctor == doctor1).ToList();

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();
        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add(filterName, filterValue);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}&doctorId={authenticatedUserId}");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();

        responseContent.Records.Should().NotBeEmpty();
        responseContent.Records.Should().OnlyContain(appointmentSearchResponse =>
            doctorAppointments.Any(a => a.Id == appointmentSearchResponse.Id));
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
        switch (filterName)
        {
            case "dateStart":
                responseContent.Records.Should().OnlyContain(a => a.Date >= DateTime.Parse(filterValue));
                break;
            case "dateEnd":
                responseContent.Records.Should().OnlyContain(a => a.Date <= DateTime.Parse(filterValue));
                break;
            case "patientId":
                responseContent.Records.Should().OnlyContain(a => a.PatientId.ToString() == filterValue);
                break;
            case "type":
                responseContent.Records.Should().OnlyContain(a => a.TypeId.ToString() == filterValue);
                break;
            case "status":
                responseContent.Records.Should().OnlyContain(a => a.StatusId.ToString() == filterValue);
                break;
        }
    }

    [Fact]
    public async Task GetAppointmentsFiltered_NoFiltersProvided_ReturnsAllAppointmentsForProvidedDoctor()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;

        var otherDoctor = new Doctor {AppUser = new AppUser {FirstName = "", LastName = ""}};
        DbContext.Doctors.Add(otherDoctor);
        var doctors = new List<Doctor> {doctor, otherDoctor};

        var patient = DbContext.Patients.First();

        var appointments = new List<Appointment>();
        for (var i = 0; i < 10; i++)
        {
            var appointment = new Appointment
            {
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctors[i % doctors.Count],
                Patient = patient,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        var doctorAppointments = appointments.Where(a => a.Doctor == doctor).ToList();

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?doctorId={doctor.Id}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();
        responseContent.Records.Should().OnlyContain(appointmentSearchResponse =>
            doctorAppointments.Any(a => a.Id == appointmentSearchResponse.Id));
        var allAuthenticatedDoctorAppointments =
            DbContext.Appointments.Where(a => a.Doctor.Id == authenticatedUserId).ToList();
        allAuthenticatedDoctorAppointments.Should().OnlyContain(
            doctorAppointment =>
                responseContent.Records.Any(responseAppointment => responseAppointment.Id == doctorAppointment.Id)
        );
    }

    [Theory]
    [InlineData("dateStart", "malformedDate")]
    [InlineData("dateEnd", "malformedDate")]
    public async Task GetAppointmentsFiltered_SingleInvalidField_ReturnsBadRequestException(
        string filterName, string filterValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;

        var patient = DbContext.Patients.First();

        var appointment = new Appointment
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            Doctor = doctor,
            Patient = patient,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add(filterName, filterValue);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(Roles.Admin)]
    public async Task GetAppointmentsFiltered_AuthorizedUserIsOtherThanDoctor_ReturnsForbidden(string roleName)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, roleName);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_NoPageSizeAndPageNumberProvided_ReturnsAllAppointments()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.First();

        const int expectedPageNumber = 1;

        DbContext.Appointments.RemoveRange(DbContext.Appointments);
        var appointments = new List<Appointment>();
        for (var i = 0; i < 10; i++)
        {
            var appointment = new Appointment
            {
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();
        responseContent.Records.Count().Should().Be(appointments.Count);
        responseContent.PageSize.Should().Be(appointments.Count);
        responseContent.PageNumber.Should().Be(expectedPageNumber);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_OnlyPageSizeIsProvided_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.Include(p => p.AppUser).First();

        const int pageSize = 5;

        var appointments = new List<Appointment>();
        for (var i = 0; i < 20; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_OnlyPageNumberIsProvided_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.First();

        const int pageNumber = 2;

        var appointments = new List<Appointment>();
        for (var i = 0; i < 20; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_PageSizeIsNegative_ResultPageSizeIsOne()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.Include(p => p.AppUser).First();

        const int pageSize = -5;
        const int pageNumber = 3;

        var appointments = new List<Appointment>();
        for (var i = 0; i < 20; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_PageNumberIsNegative_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.Include(p => p.AppUser).First();

        const int pageSize = 3;
        const int pageNumber = -2;

        var appointments = new List<Appointment>();
        for (var i = 0; i < 20; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task
        GetAppointmentsFiltered_PageSizeIsHigherThanNumberOfRecords_ResultPageSizeIsEqualToNumberOfRecords()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = await DbContext.Doctors.Include(d => d.AppUser).FirstAsync(d => d.Id == authenticatedUserId);
        var patient = DbContext.Patients.Include(p => p.AppUser).First();

        const int pageSize = 10;
        const int pageNumber = 1;
        const int expectedPageNumber = 1;

        DbContext.Appointments.RemoveRange(DbContext.Appointments);
        var appointments = new List<Appointment>();
        for (var i = 0; i < pageSize - 5; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var expectedRecords = appointments
            .Select(a => Mapper.Map<AppointmentSearchResponse>(a))
            .Skip((expectedPageNumber - 1) * appointments.Count)
            .Take(appointments.Count)
            .ToList();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();

        responseContent.Records.Should().BeEquivalentTo(expectedRecords);
        responseContent.Records.Count().Should().Be(appointments.Count);
        responseContent.PageSize.Should().Be(appointments.Count);
        responseContent.PageNumber.Should().Be(expectedPageNumber);
    }

    [Fact]
    public async Task GetAppointmentsFiltered_PageNumberIsHigherThanNumberOfRecords_ReturnsRangeNotSatisfiable()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var doctor = (await DbContext.Doctors.FindAsync(authenticatedUserId))!;
        var patient = DbContext.Patients.Include(p => p.AppUser).First();

        const int pageSize = 3;
        const int pageNumber = 25;

        var appointments = new List<Appointment>();
        for (var i = 0; i < pageNumber - 5; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Description = "Description",
                Doctor = doctor,
                Patient = patient,
                PatientId = patient.Id,
                Status = DbContext.AppointmentStatuses.First(),
                Type = DbContext.AppointmentTypes.First()
            };
            appointments.Add(appointment);
        }

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("pageSize", pageSize.ToString());
        queryString.Add("pageNumber", pageNumber.ToString());

        // act
        var response = await client.GetAsync($"{UrlPrefix}/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.RequestedRangeNotSatisfiable);
    }

    [Theory]
    [InlineData("dateStart", "2022-07-04T15:12:52Z")]
    [InlineData("dateEnd", "2022-07-04T15:12:52Z")]
    [InlineData("typeId", "e58cabc9-e259-42ff-a2a1-0e8d39bb900e")]
    [InlineData("statusId", "b7a08d2e-116d-42e3-9ec5-1aa0636d116c")]
    public async Task
        GetAppointmentsForAuthenticatedPatientFiltered_SingleFieldProvided_ReturnsAppointmentsMatchingProvidedFilter(
            string filterName, string filterValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);

        var patient1 = (await DbContext.Patients.FindAsync(authenticatedUserId))!;
        var patient2 = new Patient
        {
            AppUser = new AppUser {FirstName = "", LastName = ""},
            Address = "",
            NationalId = ""
        };
        DbContext.Patients.Add(patient2);
        var patients = new List<Patient> {patient1, patient2};

        var doctor = DbContext.Doctors.First();

        var appointmentTypes = DbContext.AppointmentTypes.ToList();
        var appointmentStatuses = DbContext.AppointmentStatuses.ToList();

        const int appointmentCount = 10;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            var appointment = new Appointment
            {
                Date = DateTime.Parse("2022-07-01T15:12:52Z").AddDays(i + 1),
                Description = "Description",
                Doctor = doctor,
                Patient = patients[i % patients.Count],
                Status = appointmentStatuses[i % appointmentStatuses.Count],
                Type = appointmentTypes[i % appointmentTypes.Count]
            };
            appointments.Add(appointment);
            DbContext.Appointments.Add(appointment);
        }

        await DbContext.Appointments.AddRangeAsync(appointments);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add(filterName, filterValue);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current/search?{queryString}");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();

        responseContent.Records.Should().NotBeEmpty();
        responseContent.Records.Should().OnlyContain(a => a.PatientId == authenticatedUserId);
        responseContent.Records.Should().BeInAscendingOrder(appointment => appointment.Date);
        switch (filterName)
        {
            case "dateStart":
                responseContent.Records.Should().OnlyContain(a => a.Date >= DateTime.Parse(filterValue));
                break;
            case "dateEnd":
                responseContent.Records.Should().OnlyContain(a => a.Date <= DateTime.Parse(filterValue));
                break;
            case "type":
                responseContent.Records.Should().OnlyContain(a => a.TypeId.ToString() == filterValue);
                break;
            case "status":
                responseContent.Records.Should().OnlyContain(a => a.StatusId.ToString() == filterValue);
                break;
        }
    }

    [Fact]
    public async Task GetAppointmentsForAuthenticatedPatientFiltered_NoFiltersProvided_ReturnsAllAppointmentsForUser()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);

        var patient1 = (await DbContext.Patients.FindAsync(authenticatedUserId))!;
        var patient2 = new Patient
        {
            AppUser = new AppUser {FirstName = "", LastName = ""},
            Address = "",
            NationalId = ""
        };
        DbContext.Patients.Add(patient2);
        var patients = new List<Patient> {patient1, patient2};

        var doctor = DbContext.Doctors.First();

        var appointmentTypes = DbContext.AppointmentTypes.ToList();
        var appointmentStatuses = DbContext.AppointmentStatuses.ToList();

        const int appointmentCount = 10;
        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            var appointment = new Appointment
            {
                Date = DateTime.Parse("2022-07-01T15:12:52Z").AddDays(i + 1),
                Description = "Description",
                Doctor = doctor,
                Patient = patients[i % patients.Count],
                Status = appointmentStatuses[i % appointmentStatuses.Count],
                Type = appointmentTypes[i % appointmentTypes.Count]
            };
            appointments.Add(appointment);
            DbContext.Appointments.Add(appointment);
        }

        await DbContext.Appointments.AddRangeAsync(appointments);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<PagedResponse<AppointmentSearchResponse>>();

        responseContent.Records.Should().OnlyContain(a => a.PatientId == authenticatedUserId);
        var allAuthenticatedPatientAppointments = DbContext.Appointments
            .Include(a => a.Status)
            .Include(a => a.Type)
            .Include(a => a.Patient)
            .ThenInclude(p => p.AppUser)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.AppUser)
            .Where(a => a.PatientId == authenticatedUserId).ToList();
        responseContent.Records.Should()
            .BeEquivalentTo(allAuthenticatedPatientAppointments.Select(a => Mapper.Map<AppointmentSearchResponse>(a)));
    }

    [Theory]
    [InlineData("dateStart", "malformedDate")]
    [InlineData("dateEnd", "malformedDate")]
    public async Task GetAppointmentsForAuthenticatedPatientFiltered_SingleInvalidField_ReturnsBadRequest(
        string filterName, string filterValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var patient = (await DbContext.Patients.FindAsync(authenticatedUserId))!;

        var doctor = DbContext.Doctors.First();

        var appointment = new Appointment
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            Doctor = doctor,
            Patient = patient,
            Status = DbContext.AppointmentStatuses.First(),
            Type = DbContext.AppointmentTypes.First()
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add(filterName, filterValue);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current/search?{queryString}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentsForAuthenticatedPatientFiltered_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Admin)]
    public async Task GetAppointmentsForAuthenticatedPatientFiltered_AuthenticatedUserOtherThanPatient_ReturnsForbidden(
        string role)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, role);

        // act
        var response = await client.GetAsync($"{UrlPrefix}/patient/current/search");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAppointment_ValidRequest_CreatesNewAppointment()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var patient = DbContext.Patients.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = authenticatedUserId,
            PatientId = patient.Id,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createAppointmentRequest);

        // assert
        RefreshDbContext();
        var createdAppointmentResponse = await response.Content.ReadAsAsync<AppointmentResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdAppointment = DbContext.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Status)
            .Include(a => a.Type)
            .First(a => a.Id == createdAppointmentResponse.Id);

        createdAppointment.Date.Should().Be(createAppointmentRequest.Date);
        createdAppointment.Description.Should().Be(createAppointmentRequest.Description);
        createdAppointment.Doctor.Id.Should().Be(createAppointmentRequest.DoctorId);
        createdAppointment.Patient.Id.Should().Be(createAppointmentRequest.PatientId);
        createdAppointment.Type.Id.Should().Be(createAppointmentRequest.TypeId);
    }

    [Theory]
    [InlineData("Description", "")]
    [InlineData("DoctorId", "")]
    [InlineData("DoctorId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("TypeId", "")]
    [InlineData("TypeId", "00000000-0000-0000-0000-000000000000")]
    public async Task CreateAppointment_SingleFieldInRequestIsInvalid_ReturnsBadRequest(
        string fieldName, object fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var patient = DbContext.Patients.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = authenticatedUserId,
            PatientId = patient.Id,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldName, out var fieldNameAsGuid);
            typeof(CreateAppointmentRequest)
                .GetProperty(fieldName)!
                .SetValue(createAppointmentRequest, fieldNameAsGuid);
        }
        else
        {
            typeof(CreateAppointmentRequest)
                .GetProperty(fieldName)!
                .SetValue(createAppointmentRequest, fieldValue);
        }

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointment_NoAuthorizedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createAppointmentRequest = new CreateAppointmentRequest();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(Roles.Admin)]
    public async Task CreateAppointment_AuthorizedUserOtherThanDoctor_ReturnsForbidden(string role)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, role);

        var createAppointmentRequest = new CreateAppointmentRequest();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAppointment_IdForOtherThanAuthorizedDoctorIsProvidedInTheRequest_ReturnsBadRequest()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsDoctorAsync(client);
        var otherDoctor = new Doctor {AppUser = new AppUser()};
        DbContext.Doctors.Add(otherDoctor);
        var patient = DbContext.Patients.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = otherDoctor.Id,
            PatientId = patient.Id,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/doctor/current", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointmentRequest_ValidRequest_CreatesNewAppointmentWithStatusPending()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var doctor = DbContext.Doctors.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = doctor.Id,
            PatientId = authenticatedUserId,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/patient/current/request", createAppointmentRequest);

        // assert
        RefreshDbContext();
        var createdAppointment = await response.Content.ReadAsAsync<AppointmentResponse>();
        var authenticatedUsersAppointments = DbContext.Appointments
            .Where(a => a.Patient.Id == authenticatedUserId)
            .ToList();

        authenticatedUsersAppointments.Should().Contain(a => a.Id == createdAppointment.Id);
        createdAppointment.StatusId.Should().Be(AppointmentStatuses.Pending.Id);
    }

    [Theory]
    [InlineData("Description", "")]
    [InlineData("DoctorId", "")]
    [InlineData("DoctorId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("PatientId", "")]
    [InlineData("PatientId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("TypeId", "")]
    [InlineData("TypeId", "00000000-0000-0000-0000-000000000000")]
    public async Task CreateAppointmentRequest_SingleFieldInRequestIsInvalid_ReturnsBadRequest(
        string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var doctor = DbContext.Doctors.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = doctor.Id,
            PatientId = authenticatedUserId,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        if (fieldName.EndsWith("Id"))
        {
            Guid.TryParse(fieldName, out var fieldNameAsGuid);
            typeof(CreateAppointmentRequest)
                .GetProperty(fieldName)!
                .SetValue(createAppointmentRequest, fieldNameAsGuid);
        }
        else
        {
            typeof(CreateAppointmentRequest)
                .GetProperty(fieldName)!
                .SetValue(createAppointmentRequest, fieldValue);
        }

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/patient/current/request", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointmentRequest_NoAuthorizedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var createAppointmentRequest = new CreateAppointmentRequest();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/patient/current/request", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Admin)]
    public async Task CreateAppointmentRequest_AuthorizedUserOtherThanPatient_ReturnsForbidden(string role)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsRoleAsync(client, role);

        var createAppointmentRequest = new CreateAppointmentRequest();

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/patient/current/request", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task
        CreateAppointmentRequest_IdForOtherThanAuthorizedPatientIsProvidedInTheRequest_CreatesAppointmentRequestForAuthenticatedUser()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);
        var otherPatient = new Patient {AppUser = new AppUser()};
        DbContext.Patients.Add(otherPatient);
        var doctor = DbContext.Doctors.First();

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            DoctorId = doctor.Id,
            PatientId = otherPatient.Id,
            TypeId = DbContext.AppointmentTypes.First().Id
        };

        // act
        var response = await client.PostAsJsonAsync($"{UrlPrefix}/patient/current/request", createAppointmentRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAppointmentById_ValidRequest_UpdatesAppointment()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = DbContext.Patients.First(u => u.Id == authenticatedUserId);
        var pendingStatus = DbContext.AppointmentStatuses.First(s => s.Id == AppointmentStatuses.Pending.Id);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            Doctor = DbContext.Doctors.First(),
            Patient = authenticatedPatient,
            Type = DbContext.AppointmentTypes.First(),
            Status = pendingStatus
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var appointmentId = appointment.Id;
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(2),
            Description = "UpdatedDescription",
            TypeId = DbContext.AppointmentTypes.Last().Id,
            StatusId = AppointmentStatuses.Cancelled.Id
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RefreshDbContext();
        var updatedAppointment = DbContext.Appointments
            .Include(a => a.Status)
            .Include(a => a.Type)
            .First(a => a.Id == appointmentId);

        updatedAppointment.Date.Should().Be(updateAppointmentRequest.Date);
        updatedAppointment.Description.Should().Be(updateAppointmentRequest.Description);
        updatedAppointment.Type.Id.Should().Be(updateAppointmentRequest.TypeId.Value);
        updatedAppointment.Status.Id.Should().Be(updateAppointmentRequest.StatusId.Value);
    }

    [Fact]
    public async Task UpdateAppointmentById_IdDoesntExist_ReturnsNotFound()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        const int appointmentId = 12345678;
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(2),
            Description = "UpdatedDescription",
            TypeId = DbContext.AppointmentTypes.Last().Id,
            StatusId = AppointmentStatuses.Cancelled.Id
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("8445a2f4-97cd-45c9-921f-f649f85cc0be")]
    [InlineData("5de8a7ba-fb65-464f-9583-181d20d44b1b")]
    [InlineData("b7a08d2e-116d-42e3-9ec5-1aa0636d116c")]
    [InlineData("1cf993e4-73f2-497f-ad38-bccb4b4d0eee")]
    public async Task UpdateAppointmentById_AuthenticatedUserIsPatientAndStatusIsNotCancelled_ReturnsBadRequest(
        string statusId)
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsPatientAsync(client);

        var appointmentId = Guid.NewGuid();
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(2),
            Description = "UpdatedDescription",
            TypeId = DbContext.AppointmentTypes.Last().Id,
            StatusId = Guid.Parse(statusId)
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAppointmentById_AuthenticatedUserIsPatientAndStatusIsCancelled_UpdatesAppointment()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = DbContext.Patients.First(u => u.Id == authenticatedUserId);
        var pendingStatus = DbContext.AppointmentStatuses.First(s => s.Id == AppointmentStatuses.Pending.Id);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            Doctor = DbContext.Doctors.First(),
            Patient = authenticatedPatient,
            Type = DbContext.AppointmentTypes.First(),
            Status = pendingStatus
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var appointmentId = appointment.Id;
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            StatusId = AppointmentStatuses.Cancelled.Id
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RefreshDbContext();
        var updatedAppointment = DbContext.Appointments
            .Include(a => a.Status)
            .First(a => a.Id == appointmentId);

        updatedAppointment.Status.Id.Should().Be(updateAppointmentRequest.StatusId.Value);
    }

    [Theory]
    [InlineData("b7a08d2e-116d-42e3-9ec5-1aa0636d116c", "5de8a7ba-fb65-464f-9583-181d20d44b1b")] // pending -> completed
    [InlineData("8445a2f4-97cd-45c9-921f-f649f85cc0be", "b7a08d2e-116d-42e3-9ec5-1aa0636d116c")] // accepted -> pending
    [InlineData("8445a2f4-97cd-45c9-921f-f649f85cc0be", "1cf993e4-73f2-497f-ad38-bccb4b4d0eee")] // accepted -> rejected
    [InlineData("1cf993e4-73f2-497f-ad38-bccb4b4d0eee", "b7a08d2e-116d-42e3-9ec5-1aa0636d116c")] // rejected -> pending
    [InlineData("1cf993e4-73f2-497f-ad38-bccb4b4d0eee", "8445a2f4-97cd-45c9-921f-f649f85cc0be")] // rejected -> accepted
    [InlineData("1cf993e4-73f2-497f-ad38-bccb4b4d0eee",
        "ccbb0db5-1661-4f9b-9482-67280ebdb6b5")] // rejected -> cancelled
    [InlineData("1cf993e4-73f2-497f-ad38-bccb4b4d0eee",
        "5de8a7ba-fb65-464f-9583-181d20d44b1b")] // rejected -> completed
    [InlineData("ccbb0db5-1661-4f9b-9482-67280ebdb6b5", "b7a08d2e-116d-42e3-9ec5-1aa0636d116c")] // cancelled -> pending
    [InlineData("ccbb0db5-1661-4f9b-9482-67280ebdb6b5",
        "8445a2f4-97cd-45c9-921f-f649f85cc0be")] // cancelled -> accepted
    [InlineData("ccbb0db5-1661-4f9b-9482-67280ebdb6b5",
        "1cf993e4-73f2-497f-ad38-bccb4b4d0eee")] // cancelled -> rejected
    [InlineData("ccbb0db5-1661-4f9b-9482-67280ebdb6b5",
        "5de8a7ba-fb65-464f-9583-181d20d44b1b")] // cancelled -> completed
    [InlineData("5de8a7ba-fb65-464f-9583-181d20d44b1b", "b7a08d2e-116d-42e3-9ec5-1aa0636d116c")] // completed -> pending
    [InlineData("5de8a7ba-fb65-464f-9583-181d20d44b1b",
        "8445a2f4-97cd-45c9-921f-f649f85cc0be")] // completed -> accepted
    [InlineData("5de8a7ba-fb65-464f-9583-181d20d44b1b",
        "1cf993e4-73f2-497f-ad38-bccb4b4d0eee")] // completed -> rejected
    [InlineData("5de8a7ba-fb65-464f-9583-181d20d44b1b",
        "ccbb0db5-1661-4f9b-9482-67280ebdb6b5")] // completed -> cancelled
    public async Task UpdateAppointmentById_AuthenticatedUserIsDoctorAndStatusTransitionIsInvalid_ReturnsBadRequest(
        string initialStatusId, string newStatusId)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsDoctorAsync(client);
        var authenticatedDoctor = DbContext.Doctors.First(u => u.Id == authenticatedUserId);
        var initialAppointmentStatus = DbContext.AppointmentStatuses.First(s => s.Id.ToString() == initialStatusId);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Description",
            Doctor = authenticatedDoctor,
            Patient = DbContext.Patients.First(),
            Type = DbContext.AppointmentTypes.First(),
            Status = initialAppointmentStatus
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var appointmentId = appointment.Id;
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            StatusId = Guid.Parse(newStatusId)
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Date", "2022-07-09T10:49:11Z")]
    [InlineData("Description", "UpdatedDescription")]
    [InlineData("TypeId", "e58cabc9-e259-42ff-a2a1-0e8d39bb900e")]
    [InlineData("StatusId", "ccbb0db5-1661-4f9b-9482-67280ebdb6b5")]
    public async Task UpdateAppointmentById_SingleFieldInRequest_UpdatesAppointment(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = DbContext.Patients.First(u => u.Id == authenticatedUserId);
        var pendingStatus = DbContext.AppointmentStatuses.First(s => s.Id == AppointmentStatuses.Pending.Id);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Parse("2022-07-07T10:49:11Z"),
            Description = "Description",
            Doctor = DbContext.Doctors.First(),
            Patient = authenticatedPatient,
            Type = DbContext.AppointmentTypes.First(),
            Status = pendingStatus
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var appointmentId = appointment.Id;
        var updateAppointmentRequest = new UpdateAppointmentRequest();

        if (fieldName == "Date")
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest,
                DateTime.Parse(fieldValue));
        else if (fieldName.EndsWith("Id"))
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest,
                Guid.Parse(fieldValue));
        else
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RefreshDbContext();
        var updatedAppointment = DbContext.Appointments
            .Include(a => a.Status)
            .Include(a => a.Type)
            .First(a => a.Id == appointmentId);

        switch (fieldName)
        {
            case nameof(UpdateAppointmentRequest.Date):
                updatedAppointment.Date.Should().Be(DateTime.Parse(fieldValue));
                break;
            case nameof(UpdateAppointmentRequest.Description):
                updatedAppointment.Description.Should().Be(fieldValue);
                break;
            case nameof(UpdateAppointmentRequest.TypeId):
                updatedAppointment.Type.Id.Should().Be(Guid.Parse(fieldValue));
                break;
            case nameof(UpdateAppointmentRequest.StatusId):
                updatedAppointment.Status.Id.Should().Be(Guid.Parse(fieldValue));
                break;
        }
    }

    [Theory]
    [InlineData("TypeId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("StatusId", "00000000-0000-0000-0000-000000000000")]
    public async Task UpdateAppointmentById_SingleFieldIsInvalid_ReturnsBadRequest(string fieldName, string fieldValue)
    {
        // arrange
        var client = await GetHttpClientAsync();
        var authenticatedUserId = await AuthenticateAsPatientAsync(client);
        var authenticatedPatient = DbContext.Patients.First(u => u.Id == authenticatedUserId);
        var pendingStatus = DbContext.AppointmentStatuses.First(s => s.Id == AppointmentStatuses.Pending.Id);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Parse("2022-07-07T10:49:11Z"),
            Description = "Description",
            Doctor = DbContext.Doctors.First(),
            Patient = authenticatedPatient,
            Type = DbContext.AppointmentTypes.First(),
            Status = pendingStatus
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        var appointmentId = appointment.Id;
        var updateAppointmentRequest = new UpdateAppointmentRequest();

        if (fieldName == "Date")
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest,
                DateTime.Parse(fieldValue));
        else if (fieldName.EndsWith("Id"))
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest,
                Guid.Parse(fieldValue));
        else
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest, fieldValue);

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAppointmentById_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        // arrange
        var client = await GetHttpClientAsync();

        var appointmentId = Guid.NewGuid();
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(2),
            Description = "UpdatedDescription",
            TypeId = DbContext.AppointmentTypes.Last().Id,
            StatusId = AppointmentStatuses.Cancelled.Id
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAppointmentById_AuthenticatedUserIsAdmin_ReturnsForbidden()
    {
        // arrange
        var client = await GetHttpClientAsync();
        await AuthenticateAsAdminAsync(client);

        var appointmentId = Guid.NewGuid();
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(2),
            Description = "UpdatedDescription",
            TypeId = DbContext.AppointmentTypes.Last().Id,
            StatusId = AppointmentStatuses.Cancelled.Id
        };

        var serializedContent = JsonConvert.SerializeObject(updateAppointmentRequest);
        var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

        // act
        var response = await client.PatchAsync($"{UrlPrefix}/user/current/{appointmentId}", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}