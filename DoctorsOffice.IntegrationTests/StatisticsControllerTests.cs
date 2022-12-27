using System.Net;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOffice.IntegrationTests;

public class StatisticsControllerTests : IntegrationTest
{
    private const string UrlPrefix = "api/statistics";

    public StatisticsControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStatisticsForAuthenticatedUser_UserIsNotAuthenticated_ReturnsUnauthenticatedError()
    {
        // arrange
        var client = await GetHttpClientAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStatisticsForAuthenticatedUser_UserIsAuthenticated_ReturnsStatistics()
    {
        // arrange
        var client = await GetHttpClientAsync();
        var doctorId = await AuthenticateAsDoctorAsync(client);
        var patient = await DbContext.Patients.FirstAsync();
        var appointment = new Appointment
        {
            DoctorId = doctorId,
            PatientId = patient.Id,
            Description = "Test",
            Date = DateTime.UtcNow
        };
        var prescription = new Prescription
        {
            DoctorId = doctorId,
            PatientId = patient.Id
        };
        await DbContext.Appointments.AddAsync(appointment);
        await DbContext.Prescriptions.AddAsync(prescription);
        await DbContext.SaveChangesAsync();

        // act
        var response = await client.GetAsync($"{UrlPrefix}/doctor/current");

        // assert
        RefreshDbContext();
        var responseContent = await response.Content.ReadAsAsync<DoctorStatisticsResponse>();
        responseContent.DoctorsPatientCount.Should().Be(1);
        responseContent.PrescriptionsCount.Should().Be(1);
    }
}