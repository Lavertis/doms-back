using System.Linq.Expressions;
using DoctorsOfficeApi.CQRS.Commands.CreateAppointment;
using DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;
using DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;
using DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using DoctorsOfficeApi.Repositories.AppointmentStatusRepository;
using DoctorsOfficeApi.Repositories.AppointmentTypeRepository;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Repositories.PatientRepository;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AppointmentHandlerTests
{
    private readonly IAppointmentRepository _fakeAppointmentRepository;
    private readonly IAppointmentStatusRepository _fakeAppointmentStatusRepository;
    private readonly IAppointmentTypeRepository _fakeAppointmentTypeRepository;
    private readonly IDoctorRepository _fakeDoctorRepository;
    private readonly IPatientRepository _fakePatientRepository;


    public AppointmentHandlerTests()
    {
        _fakeAppointmentRepository = A.Fake<IAppointmentRepository>();
        _fakeAppointmentStatusRepository = A.Fake<IAppointmentStatusRepository>();
        _fakeAppointmentTypeRepository = A.Fake<IAppointmentTypeRepository>();
        _fakeDoctorRepository = A.Fake<IDoctorRepository>();
        _fakePatientRepository = A.Fake<IPatientRepository>();
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdHandler_PatientExists_ReturnsAllAppointments()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var appointmentsQueryable = GetAppointments(3, patientId: patientId, doctorId: Guid.NewGuid()).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointmentsQueryable);

        var expectedResponse = appointmentsQueryable.Select(a => new AppointmentResponse(a));

        var query = new GetAppointmentsByPatientIdQuery(patientId);
        var handler = new GetAppointmentsByPatientIdHandler(_fakeAppointmentRepository);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdHandler_PatientDoesntExist_ReturnsEmptyList()
    {
        // arrange
        var patientId = Guid.NewGuid();

        var emptyAppointmentQueryable = A.CollectionOfDummy<Appointment>(0).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(emptyAppointmentQueryable);

        var query = new GetAppointmentsByPatientIdQuery(patientId);
        var handler = new GetAppointmentsByPatientIdHandler(_fakeAppointmentRepository);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorExists_ReturnsALlAppointments()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var appointmentsQueryable = GetAppointments(3, patientId: Guid.NewGuid(), doctorId: doctorId).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointmentsQueryable);

        var expectedResponse = appointmentsQueryable.Select(a => new AppointmentResponse(a));

        var query = new GetAppointmentsByDoctorIdQuery(doctorId);
        var handler = new GetAppointmentsByDoctorIdHandler(_fakeAppointmentRepository);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorDoesntExist_ReturnsEmptyList()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        var emptyAppointmentQueryable = A.CollectionOfDummy<Appointment>(0).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(emptyAppointmentQueryable);

        var query = new GetAppointmentsByDoctorIdQuery(doctorId);
        var handler = new GetAppointmentsByDoctorIdHandler(_fakeAppointmentRepository);

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
        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored, A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);

        var expectedResponse = new AppointmentResponse(appointment);

        var query = new GetAppointmentByIdQuery(appointment.Id);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetAppointmentByIdHandler_AppointmentDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var nonExistingAppointmentId = Guid.NewGuid();

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Throws(new NotFoundException(""));

        var query = new GetAppointmentByIdQuery(nonExistingAppointmentId);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository);

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

        var expectedResponse = appointments.Select(a => new AppointmentResponse(a));

        var query = new GetFilteredAppointmentsQuery(null, null, null, null, null, null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_DateEndBeforeDateStart_ReturnsEmptyList()
    {
        // arrange
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };
        const int appointmentCount = 3;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var query = new GetFilteredAppointmentsQuery(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.Subtract(1.Days()),
            null,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        const string invalidType = "InvalidType";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            invalidType,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        const string invalidStatus = "InvalidStatus";

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            invalidStatus,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var invalidPatientId = Guid.NewGuid();

        var query = new GetFilteredAppointmentsQuery
        {
            patientId = invalidPatientId
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var invalidDoctorId = Guid.NewGuid();

        var query = new GetFilteredAppointmentsQuery
        {
            doctorId = invalidDoctorId
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidDateRange_ReturnsAppointmentsInDateRange()
    {
        // arrange
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };
        const int appointmentCount = 5;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var dateStart = DateTime.UtcNow.AddDays(1);
        var dateEnd = DateTime.UtcNow.AddDays(2);

        var query = new GetFilteredAppointmentsQuery(
            dateStart,
            dateEnd,
            null,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var types = new List<AppointmentType>
        {
            new() { Id = Guid.NewGuid(), Name = "Type1" },
            new() { Id = Guid.NewGuid(), Name = "Type2" },
            new() { Id = Guid.NewGuid(), Name = "Type2" }
        };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = status,
                Type = types[i % types.Count],
                Description = ""
            });
        }

        var selectedType = types[1];

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            selectedType.Name,
            null,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var statuses = new List<AppointmentStatus>
        {
            new() { Id = Guid.NewGuid(), Name = "Status1" },
            new() { Id = Guid.NewGuid(), Name = "Status2" },
            new() { Id = Guid.NewGuid(), Name = "Status2" }
        };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patient,
                Status = statuses[i % statuses.Count],
                Type = type,
                Description = ""
            });
        }

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var selectedStatus = statuses[1];

        var query = new GetFilteredAppointmentsQuery(
            null,
            null,
            null,
            selectedStatus.Name,
            null,
            null);
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patients = new List<Patient>
        {
            new() { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" },
            new() { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" },
            new() { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" }
        };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctor,
                Patient = patients[i % patients.Count],
                PatientId = patients[i % patients.Count].Id,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var selectedPatient = patients[1];

        var query = new GetFilteredAppointmentsQuery
        {
            patientId = selectedPatient.Id
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var doctors = new List<Doctor>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };
        const int appointmentCount = 10;

        var appointments = new List<Appointment>();
        for (var i = 0; i < appointmentCount; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(i),
                Doctor = doctors[i % doctors.Count],
                DoctorId = doctors[i % doctors.Count].Id,
                Patient = patient,
                Status = status,
                Type = type,
                Description = ""
            });
        }

        A.CallTo(() => _fakeAppointmentRepository.GetAll(
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointments.AsQueryable().BuildMock());

        var selectedDoctor = doctors[1];

        var query = new GetFilteredAppointmentsQuery
        {
            doctorId = selectedDoctor.Id
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository);

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
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(doctor);
        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(status);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(type);

        var createAppointmentCommand = new CreateAppointmentCommand
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Status = status.Name,
            Type = type.Name,
            Description = ""
        };
        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(createAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.CreateAsync(A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("DoctorId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("PatientId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("Status", "NonExistingStatus")]
    [InlineData("Type", "NonExistingType")]
    public async Task CreateAppointmentHandler_ContainsInvalidField_ThrowsBadRequestException(string fieldName, string fieldValue)
    {
        // arrange
        var doctor = new Doctor { Id = Guid.NewGuid() };
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = Guid.NewGuid(), Name = "Pending" };
        var type = new AppointmentType { Id = Guid.NewGuid(), Name = "Consultation" };

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
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(status);

        if (fieldName == "Type")
            A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(type);

        if (fieldName == "PatientId")
            A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);

        if (fieldName == "DoctorId")
            A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(doctor);

        if (Guid.TryParse(fieldValue, out var guid))
            typeof(CreateAppointmentCommand).GetProperty(fieldName)!.SetValue(createAppointmentCommand, guid);
        else
            typeof(CreateAppointmentCommand).GetProperty(fieldName)!.SetValue(createAppointmentCommand, fieldValue);

        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

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
        var newAppointmentType = new AppointmentType { Id = Guid.NewGuid(), Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = Guid.NewGuid(), Name = "new status" };

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Type = newAppointmentType.Name,
            Description = "newDescription",
            Status = newAppointmentStatus.Name
        };

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void UpdateAppointmentHandler_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Throws(new NotFoundException(""));

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(100),
            Type = "new type",
            Description = "newDescription",
            Status = "new status"
        };

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

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
        var newAppointmentType = new AppointmentType { Id = Guid.NewGuid(), Name = "new type" };
        var newAppointmentStatus = new AppointmentStatus { Id = Guid.NewGuid(), Name = "new status" };

        var updateAppointmentCommand = new UpdateAppointmentCommand { AppointmentId = appointment.Id };

        if (fieldName == "Date")
            updateAppointmentCommand.Date = DateTime.Parse(fieldValue);
        else
            typeof(UpdateAppointmentCommand).GetProperty(fieldName)!.SetValue(updateAppointmentCommand, fieldValue);

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_RequestedTypeDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Type = "nonExistingType",
            Description = ""
        };

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(new AppointmentStatus());
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

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

        var updateAppointmentCommand = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            Date = DateTime.UtcNow.AddDays(100),
            Status = "nonExistingStatus",
            Description = ""
        };

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Throws(new NotFoundException(""));
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(new AppointmentType());

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var action = async () => await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    private static IList<Appointment> GetAppointments(
        int count,
        Guid patientId = default,
        Guid doctorId = default,
        Guid appointmentStatusId = default,
        Guid appointmentTypeId = default)
    {
        if (patientId == default)
            patientId = Guid.NewGuid();
        if (doctorId == default)
            doctorId = Guid.NewGuid();
        if (appointmentStatusId == default)
            appointmentStatusId = Guid.NewGuid();
        if (appointmentTypeId == default)
            appointmentTypeId = Guid.NewGuid();

        var doctor = new Doctor { Id = doctorId };
        var patient = new Patient { Id = patientId, FirstName = "", LastName = "", Address = "" };
        var status = new AppointmentStatus { Id = appointmentStatusId, Name = "Pending" };
        var type = new AppointmentType { Id = appointmentTypeId, Name = "Consultation" };

        var appointments = new List<Appointment>();
        for (var i = 0; i < count; i++)
        {
            appointments.Add(new Appointment()
            {
                Id = Guid.NewGuid(),
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