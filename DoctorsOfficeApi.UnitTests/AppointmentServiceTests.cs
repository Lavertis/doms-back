using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Services.AppointmentService;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentStatusEntity = DoctorsOfficeApi.Entities.AppointmentStatus;
using AppointmentTypeEntity = DoctorsOfficeApi.Entities.AppointmentType;

namespace DoctorsOfficeApi.UnitTests;

public class AppointmentServiceTests
{
    AppointmentService _appointmentService;
    AppDbContext _appDbContext;

    public AppointmentServiceTests()
    {
        var inMemoryDbName = "InMemoryDb_" + DateTime.Now.ToFileTimeUtc();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _appDbContext = new AppDbContext(dbContextOptions);
        _appointmentService = new AppointmentService(_appDbContext);
    }

    [Fact]
    public async Task GetAllAppointments_AppointmentsExist_ReturnsAllAppointments()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAllAppointmentsAsync();

        // assert
        result.Should().BeEquivalentTo(appointments);
    }

    [Fact]
    public async Task GetAllAppointments_AppointmentsDoNotExist_ReturnsEmptyList()
    {
        // assert

        // act
        var result = await _appointmentService.GetAllAppointmentsAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByPatientId_PatientExists_ReturnsAllAppointments()
    {
        // arrange
        const string patientId = "1";
        var appointments = GetAppointments(3, patientId: patientId, doctorId: patientId + "_doc");
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);

        // assert
        result.Should().BeEquivalentTo(appointments);
    }

    [Fact]
    public async Task GetAppointmentsByPatientId_PatientDoesntExist_ReturnsEmptyList()
    {
        // arrange
        const string patientId = "1";

        // act
        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByDoctorId_DoctorExists_ReturnsALlAppointments()
    {
        // arrange
        const string doctorId = "1";
        var appointments = GetAppointments(3, patientId: doctorId + "_pat", doctorId: doctorId);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);

        // assert
        result.Should().BeEquivalentTo(appointments);
    }

    [Fact]
    public async Task GetAppointmentsByDoctorId_DoctorDoesntExist_ReturnsEmptyList()
    {
        // arrange
        const string doctorId = "1";

        // act
        var result = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentById_AppointmentExists_ReturnsAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        await _appDbContext.AddAsync(appointment);
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
    public async Task GetFilteredAppointments_EveryFilterIsNull_ReturnsAllAppointments()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null, null, null, null, null, null);

        // assert
        result.Should().BeEquivalentTo(appointments);
    }

    [Fact]
    public async Task GetFilteredAppointments_DateEndBeforeDateStart_ReturnsEmptyList()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };
        const int appointmentCount = 3;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
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

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.Subtract(1.Days()),
            null,
            null,
            null,
            null
        );

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointments_InvalidType_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidType = "InvalidType";

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            invalidType,
            null,
            null,
            null
        );

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointments_InvalidStatus_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidStatus = "InvalidStatus";

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            invalidStatus,
            null,
            null
        );

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointments_InvalidPatientId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidPatientId = "invalidPatientId";

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            null,
            invalidPatientId,
            null
        );

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointments_InvalidDoctorId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidDoctorId = "invalidDoctorId";

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            null,
            null,
            invalidDoctorId
        );

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointments_ValidDateRange_ReturnsAppointmentsInDateRange()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };
        const int appointmentCount = 5;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var dateStart = DateTime.UtcNow.AddDays(1);
        var dateEnd = DateTime.UtcNow.AddDays(2);

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            dateStart,
            dateEnd,
            null,
            null,
            null,
            null
        );

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Date >= dateStart && a.Date <= dateEnd);
    }

    [Fact]
    public async Task GetFilteredAppointments_ValidType_ReturnsAppointmentsMatchingType()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var types = new List<AppointmentType>
        {
            new() { Id = 1, Name = "Type1" },
            new() { Id = 2, Name = "Type2" },
            new() { Id = 3, Name = "Type2" }
        };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = types[i % types.Count],
                Description = ""
            });
        }

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var selectedType = types[1];

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            selectedType.Name,
            null,
            null,
            null
        );

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Type.Name == selectedType.Name);
    }

    [Fact]
    public async Task GetFilteredAppointments_ValidStatus_ReturnsAppointmentsMatchingStatus()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var statuses = new List<AppointmentStatus>
        {
            new() { Id = 1, Name = "Status1" },
            new() { Id = 2, Name = "Status2" },
            new() { Id = 3, Name = "Status2" }
        };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = statuses[i % statuses.Count],
                Type = type,
                Description = ""
            });
        }

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var selectedStatus = statuses[1];

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            selectedStatus.Name,
            null,
            null
        );

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Status.Name == selectedStatus.Name);
    }

    [Fact]
    public async Task GetFilteredAppointments_ValidPatientId_ReturnsAppointmentsBelongingToPatient()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patients = new List<Patient>
        {
            new() { Id = "1" },
            new() { Id = "2" },
            new() { Id = "3" }
        };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patients[i % patients.Count],
                Status = status,
                Type = type,
                Description = ""
            });
        }

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var selectedPatient = patients[1];

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            null,
            selectedPatient.Id,
            null
        );

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Patient.Id == selectedPatient.Id);
    }

    [Fact]
    public async Task GetFilteredAppointments_ValidDoctorId_ReturnsAppointmentsBelongingToDoctor()
    {
        // arrange
        var patient = new Patient { Id = "1" };
        var doctors = new List<Doctor>
        {
            new() { Id = "1" },
            new() { Id = "2" },
            new() { Id = "3" }
        };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctors[i % doctors.Count],
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var selectedDoctor = doctors[1];

        // act
        var result = await _appointmentService.GetFilteredAppointmentsAsync(
            null,
            null,
            null,
            null,
            null,
            selectedDoctor.Id
        );

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Doctor.Id == selectedDoctor.Id);
    }

    [Fact]
    public async Task CreateAppointment_ValidRequest_CreatesNewAppointment()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Status = status.Name,
            Type = type.Name,
            Description = ""
        };

        _appDbContext.Doctors.Add(doctor);
        _appDbContext.Patients.Add(patient);
        _appDbContext.AppointmentStatuses.Add(status);
        _appDbContext.AppointmentTypes.Add(type);
        await _appDbContext.SaveChangesAsync();

        // act
        var result = await _appointmentService.CreateAppointmentAsync(createAppointmentRequest);

        // assert
        result.Should().NotBeNull();
        _appDbContext.Appointments.Should().NotBeEmpty();
        _appDbContext.Appointments.Should().ContainSingle(a => a.Id == result.Id);
        _appDbContext.Appointments.Should().ContainSingle(a =>
            a.Id == result.Id &&
            a.Date == createAppointmentRequest.Date &&
            a.Doctor.Id == createAppointmentRequest.DoctorId &&
            a.Patient.Id == createAppointmentRequest.PatientId &&
            a.Status.Name == createAppointmentRequest.Status &&
            a.Type.Name == createAppointmentRequest.Type &&
            a.Description == createAppointmentRequest.Description
        );
    }

    [Theory]
    [InlineData("DoctorId", "NonExistingDoctorId")]
    [InlineData("PatientId", "NonExistingPatientId")]
    [InlineData("Status", "NonExistingStatus")]
    [InlineData("Type", "NonExistingType")]
    public async Task CreateAppointment_ContainsInvalidField_ThrowsBadRequestException(string fieldName, string fieldValue)
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };

        var createAppointmentRequest = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Status = status.Name,
            Type = type.Name,
            Description = ""
        };

        typeof(CreateAppointmentRequest).GetProperty(fieldName)!.SetValue(createAppointmentRequest, fieldValue);

        _appDbContext.Doctors.Add(doctor);
        _appDbContext.Patients.Add(patient);
        _appDbContext.AppointmentStatuses.Add(status);
        _appDbContext.AppointmentTypes.Add(type);
        await _appDbContext.SaveChangesAsync();

        // act
        var action = async () => await _appointmentService.CreateAppointmentAsync(createAppointmentRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateAppointmentById_IdAndRequestAreValid_UpdatesAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType { Id = 2, Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = 2, Name = "new status" };

        await _appDbContext.AppointmentTypes.AddAsync(newAppointmentType);
        await _appDbContext.AppointmentStatuses.AddAsync(newAppointmentStatus);
        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = newAppointmentType.Name,
            Description = "newDescription",
            Status = newAppointmentStatus.Name
        };
        var appointmentId = appointment.Id;

        // act
        var result = await _appointmentService.UpdateAppointmentByIdAsync(appointmentId, updateAppointmentRequest);

        // assert
        _appDbContext.Appointments.Should().ContainSingle(a =>
            a.Id == appointmentId &&
            a.Date == updateAppointmentRequest.Date &&
            a.Type.Name == updateAppointmentRequest.Type &&
            a.Status.Name == updateAppointmentRequest.Status &&
            a.Description == updateAppointmentRequest.Description
        );
    }

    [Fact]
    public void UpdateAppointmentById_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow,
            Type = "dummyType",
            Description = ""
        };
        const int appointmentId = 100;

        // act
        var action = async () => await _appointmentService.UpdateAppointmentByIdAsync(appointmentId, updateAppointmentRequest);

        // assert
        action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("Date", "2022-07-03T12:12:52Z")]
    [InlineData("Type", "new type")]
    [InlineData("Status", "new status")]
    [InlineData("Description", "new description")]
    public async Task UpdateAppointmentById_SingleFieldIsPresent_UpdatesRequestedField(string fieldName, string fieldValue)
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType { Id = 2, Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = 2, Name = "new status" };

        await _appDbContext.AppointmentTypes.AddAsync(newAppointmentType);
        await _appDbContext.AppointmentStatuses.AddAsync(newAppointmentStatus);
        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentRequest = new UpdateAppointmentRequest();
        if (fieldName == "Date")
            updateAppointmentRequest.Date = DateTime.Parse(fieldValue);
        else
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(updateAppointmentRequest, fieldValue);
        var appointmentId = appointment.Id;

        // act
        var result = await _appointmentService.UpdateAppointmentByIdAsync(appointmentId, updateAppointmentRequest);

        // assert
        if (updateAppointmentRequest.Date != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == appointmentId &&
                a.Date == updateAppointmentRequest.Date
            );
        if (updateAppointmentRequest.Type != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == appointmentId &&
                a.Type.Name == updateAppointmentRequest.Type
            );
        if (updateAppointmentRequest.Status != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == appointmentId &&
                a.Status.Name == updateAppointmentRequest.Status
            );
        if (updateAppointmentRequest.Description != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == appointmentId &&
                a.Description == updateAppointmentRequest.Description
            );
    }

    [Fact]
    public async Task UpdateAppointmentById_RequestedTypeDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = "nonExistingType",
            Description = ""
        };
        var appointmentId = appointment.Id;

        // act
        var action = async () => await _appointmentService.UpdateAppointmentByIdAsync(appointmentId, updateAppointmentRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }


    [Fact]
    public async Task UpdateAppointmentById_RequestedStatusDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentRequest = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Status = "nonExistingStatus",
            Description = ""
        };
        var appointmentId = appointment.Id;

        // act
        var action = async () => await _appointmentService.UpdateAppointmentByIdAsync(appointmentId, updateAppointmentRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    private static IList<Appointment> GetAppointments(
        int count,
        string patientId = "1",
        string doctorId = "2",
        long appointmentStatusId = 1,
        long appointmentTypeId = 1)
    {
        var doctor = new Doctor { Id = doctorId };
        var patient = new Patient { Id = patientId };
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