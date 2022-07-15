using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.AppointmentService;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentStatusEntity = DoctorsOfficeApi.Entities.AppointmentStatus;
using AppointmentTypeEntity = DoctorsOfficeApi.Entities.AppointmentType;

namespace DoctorsOfficeApi.UnitTests;

public class AppointmentServiceTests
{
    private readonly AppointmentService _appointmentService;
    private readonly AppDbContext _appDbContext;

    public AppointmentServiceTests()
    {
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _appDbContext = new AppDbContext(dbContextOptions);
        _appointmentService = new AppointmentService(_appDbContext);
    }

    [Fact]
    public async Task GetAppointmentById_AppointmentExists_ReturnsAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        await _appDbContext.Appointments.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAppointmentByIdAsync(appointment.Id);

        // assert
        result.Should().BeEquivalentTo(appointment);
    }

    [Fact]
    public void GetAppointmentById_AppointmentDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const long nonExistingAppointmentId = 100;

        // act
        var action = async () => await _appointmentService.GetAppointmentByIdAsync(nonExistingAppointmentId);

        // assert
        action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AppointmentTypeExists_TypeExists_ReturnsTrue()
    {
        // arrange
        const string appointmentTypeName = "TestAppointmentType";
        await _appDbContext.AppointmentTypes.AddAsync(new AppointmentType
        {
            Id = 1,
            Name = appointmentTypeName
        });
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.AppointmentTypeExistsAsync(appointmentTypeName);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AppointmentTypeExists_TypeDoesntExist_ReturnsFalse()
    {
        // arrange
        const string appointmentTypeName = "TestAppointmentType";

        // act
        var result = await _appointmentService.AppointmentTypeExistsAsync(appointmentTypeName);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AppointmentStatusExists_StatusExists_ReturnsTrue()
    {
        // arrange
        const string appointmentStatusName = "TestAppointmentStatus";
        _appDbContext.AppointmentStatuses.Add(new AppointmentStatus
        {
            Id = 1,
            Name = appointmentStatusName
        });
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.AppointmentStatusExistsAsync(appointmentStatusName);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AppointmentStatusExists_StatusDoesntExist_ReturnsFalse()
    {
        // arrange
        const string appointmentStatusName = "TestAppointmentStatus";

        // act
        var result = await _appointmentService.AppointmentStatusExistsAsync(appointmentStatusName);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAppointmentStatusByName_StatusExists_ReturnsStatus()
    {
        // arrange
        var appointmentStatus = new AppointmentStatus
        {
            Id = 1,
            Name = "TestAppointmentStatus"
        };
        _appDbContext.AppointmentStatuses.Add(appointmentStatus);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAppointmentStatusByNameAsync(appointmentStatus.Name);

        // assert
        result.Should().BeEquivalentTo(appointmentStatus);
    }

    [Fact]
    public async Task GetAppointmentStatusByName_StatusDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string appointmentStatusName = "TestAppointmentStatus";

        // act
        var action = async () => await _appointmentService.GetAppointmentStatusByNameAsync(appointmentStatusName);

        // assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAppointmentTypeByName_TypeExists_ReturnsType()
    {
        // arrange
        var appointmentType = new AppointmentType
        {
            Id = 1,
            Name = "TestAppointmentType"
        };
        _appDbContext.AppointmentTypes.Add(appointmentType);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAppointmentTypeByNameAsync(appointmentType.Name);

        // assert
        result.Should().BeEquivalentTo(appointmentType);
    }

    [Fact]
    public async Task GetAppointmentTypeByName_AppointmentTypeDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string appointmentTypeName = "TestAppointmentType";

        // act
        var action = async () => await _appointmentService.GetAppointmentTypeByNameAsync(appointmentTypeName);

        // assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    private static IList<Appointment> GetAppointments(
        int count,
        string patientId = "1",
        string doctorId = "2",
        long appointmentStatusId = 1,
        long appointmentTypeId = 1)
    {
        var doctor = new Doctor { Id = doctorId };
        var patient = new Patient { Id = patientId, FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = appointmentStatusId, Name = "Pending" };
        var type = new AppointmentType { Id = appointmentTypeId, Name = "Consultation" };

        var appointments = new List<Appointment>();
        for (var i = 0; i < count; i++)
        {
            appointments.Add(new Appointment()
            {
                Id = i + 1,
                Date = DateTime.UtcNow,
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        return appointments;
    }
}