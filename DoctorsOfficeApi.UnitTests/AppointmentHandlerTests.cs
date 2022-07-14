﻿using DoctorsOfficeApi.CQRS.Commands.CreateAppointment;
using DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;
using DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.DoctorService;
using DoctorsOfficeApi.Services.PatientService;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AppointmentHandlerTests
{
    private readonly AppDbContext _appDbContext;
    private readonly IAppointmentService _fakeAppointmentService;
    private readonly IPatientService _fakePatientService;
    private readonly IDoctorService _fakeDoctorService;

    public AppointmentHandlerTests()
    {
        var inMemoryDbName = "InMemoryDb_" + DateTime.Now.ToFileTimeUtc();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _appDbContext = new AppDbContext(dbContextOptions);

        _fakeAppointmentService = A.Fake<IAppointmentService>();
        _fakePatientService = A.Fake<IPatientService>();
        _fakeDoctorService = A.Fake<IDoctorService>();
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdHandler_PatientExists_ReturnsAllAppointments()
    {
        // arrange
        const string patientId = "1";
        var appointments = GetAppointments(3, patientId: patientId, doctorId: patientId + "_doc");
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var expectedResponse = appointments.Select(a => new AppointmentResponse(a));

        var query = new GetAppointmentsByPatientIdQuery(patientId);
        var handler = new GetAppointmentsByPatientIdHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdHandler_PatientDoesntExist_ReturnsEmptyList()
    {
        // arrange
        const string patientId = "1";

        var query = new GetAppointmentsByPatientIdQuery(patientId);
        var handler = new GetAppointmentsByPatientIdHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorExists_ReturnsALlAppointments()
    {
        // arrange
        const string doctorId = "1";
        var appointments = GetAppointments(3, patientId: doctorId + "_pat", doctorId: doctorId);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var expectedResponse = appointments.Select(a => new AppointmentResponse(a));

        var query = new GetAppointmentsByDoctorIdQuery(doctorId);
        var handler = new GetAppointmentsByDoctorIdHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorDoesntExist_ReturnsEmptyList()
    {
        // arrange
        const string doctorId = "1";

        var query = new GetAppointmentsByDoctorIdQuery(doctorId);
        var handler = new GetAppointmentsByDoctorIdHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentByIdHandler_AppointmentExists_ReturnsAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored)).Returns(appointment);

        var expectedResponse = new AppointmentResponse(appointment);

        var query = new GetAppointmentByIdQuery(appointment.Id);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentService);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetAppointmentByIdHandler_AppointmentDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const long nonExistingAppointmentId = 100;

        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored)).Throws(new NotFoundException(""));

        var query = new GetAppointmentByIdQuery(nonExistingAppointmentId);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentService);

        // act
        var action = async () => await handler.Handle(query, CancellationToken.None);

        // assert
        action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_EveryFilterIsNull_ReturnsAllAppointments()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        var expectedResponse = appointments.Select(a => new AppointmentResponse(a));

        var query = new GetFilteredAppointmentsQuery(null, null, null, null, null, null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_DateEndBeforeDateStart_ReturnsEmptyList()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
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

        var query = new GetFilteredAppointmentsQuery(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.Subtract(1.Days()),
            null,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidType_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidType = "InvalidType";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            invalidType,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidStatus_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidStatus = "InvalidStatus";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            invalidStatus,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidPatientId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidPatientId = "invalidPatientId";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            null,
            invalidPatientId,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidDoctorId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        _appDbContext.AddRange(appointments);
        await _appDbContext.SaveChangesAsync();

        const string invalidDoctorId = "invalidDoctorId";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            null,
            null,
            invalidDoctorId);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidDateRange_ReturnsAppointmentsInDateRange()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
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

        var query = new GetFilteredAppointmentsQuery(
            dateStart,
            dateEnd,
            null,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Date >= dateStart && a.Date <= dateEnd);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidType_ReturnsAppointmentsMatchingType()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
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

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            selectedType.Name,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Type == selectedType.Name);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidStatus_ReturnsAppointmentsMatchingStatus()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
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

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            selectedStatus.Name,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.Status == selectedStatus.Name);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidPatientId_ReturnsAppointmentsBelongingToPatient()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patients = new List<Patient>
        {
            new() { Id = "1", FirstName = "", LastName = "", Address = "" },
            new() { Id = "2", FirstName = "", LastName = "", Address = "" },
            new() { Id = "3", FirstName = "", LastName = "", Address = "" }
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

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            null,
            selectedPatient.Id,
            null);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.PatientId == selectedPatient.Id);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidDoctorId_ReturnsAppointmentsBelongingToDoctor()
    {
        // arrange
        var patient = new Patient { Id = "1", FirstName = "", LastName = "", Address = "" };
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

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            null,
            null,
            selectedDoctor.Id);
        var handler = new GetFilteredAppointmentsHandler(_appDbContext);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(a => a.DoctorId == selectedDoctor.Id);
    }

    [Fact]
    public async Task CreateAppointmentHandler_ValidRequest_CreatesNewAppointment()
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };

        _appDbContext.Doctors.Add(doctor);
        _appDbContext.Patients.Add(patient);
        _appDbContext.AppointmentStatuses.Add(status);
        _appDbContext.AppointmentTypes.Add(type);
        await _appDbContext.SaveChangesAsync();

        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored)).Returns(status);
        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored)).Returns(type);
        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<string>.Ignored)).Returns(patient);
        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Returns(doctor);

        var createAppointmentCommand = new CreateAppointmentCommand
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Status = status.Name,
            Type = type.Name,
            Description = ""
        };
        var handler = new CreateAppointmentHandler(_appDbContext, _fakeAppointmentService, _fakePatientService, _fakeDoctorService);

        // act
        var result = await handler.Handle(createAppointmentCommand, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        _appDbContext.Appointments.Should().NotBeEmpty();
        _appDbContext.Appointments.Should().ContainSingle(a => a.Id == result.Id);
        _appDbContext.Appointments.Should().ContainSingle(a =>
            a.Id == result.Id &&
            a.Date == createAppointmentCommand.Date &&
            a.Doctor.Id == createAppointmentCommand.DoctorId &&
            a.Patient.Id == createAppointmentCommand.PatientId &&
            a.Status.Name == createAppointmentCommand.Status &&
            a.Type.Name == createAppointmentCommand.Type &&
            a.Description == createAppointmentCommand.Description
        );
    }

    [Theory]
    [InlineData("DoctorId", "NonExistingDoctorId")]
    [InlineData("PatientId", "NonExistingPatientId")]
    [InlineData("Status", "NonExistingStatus")]
    [InlineData("Type", "NonExistingType")]
    public async Task CreateAppointmentHandler_ContainsInvalidField_ThrowsBadRequestException(string fieldName, string fieldValue)
    {
        // arrange
        var doctor = new Doctor { Id = "1" };
        var patient = new Patient { Id = "2", FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = 1, Name = "Pending" };
        var type = new AppointmentType { Id = 1, Name = "Consultation" };

        _appDbContext.Doctors.Add(doctor);
        _appDbContext.Patients.Add(patient);
        _appDbContext.AppointmentStatuses.Add(status);
        _appDbContext.AppointmentTypes.Add(type);
        await _appDbContext.SaveChangesAsync();

        var createAppointmentCommand = new CreateAppointmentCommand
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Status = status.Name,
            Type = type.Name,
            Description = ""
        };

        if (fieldName == "Status")
            A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored)).Returns(status);

        if (fieldName == "Type")
            A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored)).Returns(type);

        if (fieldName == "PatientId")
            A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<string>.Ignored)).Returns(patient);

        if (fieldName == "DoctorId")
            A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Returns(doctor);

        typeof(CreateAppointmentCommand).GetProperty(fieldName)!.SetValue(createAppointmentCommand, fieldValue);

        var handler = new CreateAppointmentHandler(_appDbContext, _fakeAppointmentService, _fakePatientService, _fakeDoctorService);

        // act
        var action = async () => await handler.Handle(createAppointmentCommand, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_IdAndRequestAreValid_UpdatesAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType { Id = 2, Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = 2, Name = "new status" };

        await _appDbContext.AppointmentTypes.AddAsync(newAppointmentType);
        await _appDbContext.AppointmentStatuses.AddAsync(newAppointmentStatus);
        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Type = newAppointmentType.Name,
            Description = "newDescription",
            Status = newAppointmentStatus.Name
        };

        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);
        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored)).Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored)).Returns(appointment);


        var handler = new UpdateAppointmentHandler(_appDbContext, _fakeAppointmentService);

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        _appDbContext.Appointments.Should().ContainSingle(a =>
            a.Id == updateAppointmentCommand.AppointmentId &&
            a.Date == updateAppointmentCommand.Date &&
            a.Type.Name == updateAppointmentCommand.Type &&
            a.Status.Name == updateAppointmentCommand.Status &&
            a.Description == updateAppointmentCommand.Description
        );
    }

    [Fact]
    public void UpdateAppointmentHandler_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = 1,
            Date = DateTime.UtcNow.AddDays(100),
            Type = "new type",
            Description = "newDescription",
            Status = "new status"
        };

        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentType());
        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentStatus());
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored))
            .Throws(new NotFoundException(""));

        var handler = new UpdateAppointmentHandler(_appDbContext, _fakeAppointmentService);

        // act
        var action = async () => await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("Date", "2022-07-03T12:12:52Z")]
    [InlineData("Type", "new type")]
    [InlineData("Status", "new status")]
    [InlineData("Description", "new description")]
    public async Task UpdateAppointmentHandler_SingleFieldIsPresent_UpdatesRequestedField(string fieldName, string fieldValue)
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType { Id = 2, Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = 2, Name = "new status" };

        await _appDbContext.AppointmentTypes.AddAsync(newAppointmentType);
        await _appDbContext.AppointmentStatuses.AddAsync(newAppointmentStatus);
        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentCommand = new UpdateAppointmentCommand { AppointmentId = appointment.Id };

        if (fieldName == "Date")
            updateAppointmentCommand.Date = DateTime.Parse(fieldValue);
        else
            typeof(UpdateAppointmentCommand).GetProperty(fieldName)!.SetValue(updateAppointmentCommand, fieldValue);

        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);
        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored)).Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored)).Returns(appointment);


        var handler = new UpdateAppointmentHandler(_appDbContext, _fakeAppointmentService);

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        if (updateAppointmentCommand.Date != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == updateAppointmentCommand.AppointmentId &&
                a.Date == updateAppointmentCommand.Date
            );
        if (updateAppointmentCommand.Type != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == updateAppointmentCommand.AppointmentId &&
                a.Type.Name == updateAppointmentCommand.Type
            );
        if (updateAppointmentCommand.Status != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == updateAppointmentCommand.AppointmentId &&
                a.Status.Name == updateAppointmentCommand.Status
            );
        if (updateAppointmentCommand.Description != null)
            _appDbContext.Appointments.Should().ContainSingle(a =>
                a.Id == updateAppointmentCommand.AppointmentId &&
                a.Description == updateAppointmentCommand.Description
            );
    }

    [Fact]
    public async Task UpdateAppointmentHandler_RequestedTypeDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Type = "nonExistingType",
            Description = ""
        };

        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));
        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentStatus());
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored))
            .Returns(appointment);

        var handler = new UpdateAppointmentHandler(_appDbContext, _fakeAppointmentService);

        // act
        var action = async () => await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }


    [Fact]
    public async Task UpdateAppointmentHandler_RequestedStatusDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        await _appDbContext.AddAsync(appointment);
        await _appDbContext.SaveChangesAsync();

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Status = "nonExistingStatus",
            Description = ""
        };

        A.CallTo(() => _fakeAppointmentService.GetAppointmentTypeByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentType());
        A.CallTo(() => _fakeAppointmentService.GetAppointmentStatusByNameAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));
        A.CallTo(() => _fakeAppointmentService.GetAppointmentByIdAsync(A<long>.Ignored))
            .Returns(appointment);

        var handler = new UpdateAppointmentHandler(_appDbContext, _fakeAppointmentService);

        // act
        var action = async () => await handler.Handle(updateAppointmentCommand, CancellationToken.None);

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