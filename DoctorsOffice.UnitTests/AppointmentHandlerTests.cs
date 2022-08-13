using System.Linq.Expressions;
using DoctorsOffice.Application.CQRS.Commands.CreateAppointment;
using DoctorsOffice.Application.CQRS.Commands.UpdateAppointment;
using DoctorsOffice.Application.CQRS.Queries.GetAppointmentById;
using DoctorsOffice.Application.CQRS.Queries.GetAppointmentsByUser;
using DoctorsOffice.Application.CQRS.Queries.GetFilteredAppointments;
using DoctorsOffice.Application.Services.Appointment;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AppointmentHandlerTests
{
    private readonly IAppointmentRepository _fakeAppointmentRepository;
    private readonly IAppointmentService _fakeAppointmentService;
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
        _fakeAppointmentService = A.Fake<IAppointmentService>();
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

        var query = new GetAppointmentsByUserQuery(patientId, RoleTypes.Patient);
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository);

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

        var query = new GetAppointmentsByUserQuery(patientId, RoleTypes.Patient);
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository);

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

        var query = new GetAppointmentsByUserQuery(doctorId, RoleTypes.Doctor);
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository);

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

        var query = new GetAppointmentsByUserQuery(doctorId, RoleTypes.Doctor);
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository);

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
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentService.CanUserAccessAppointment(
            A<Guid>.Ignored,
            A<string>.Ignored,
            A<Guid>.Ignored,
            A<Guid>.Ignored
        )).Returns(true);

        var expectedResponse = new AppointmentResponse(appointment);

        var query = new GetAppointmentByIdQuery(appointment.Id, Guid.NewGuid(), RoleTypes.Admin);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository, _fakeAppointmentService);

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

        var query = new GetAppointmentByIdQuery(nonExistingAppointmentId, Guid.NewGuid(), string.Empty);
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository, _fakeAppointmentService);

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

        var request = new GetAppointmentsFilteredRequest();
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};
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

        var request = new GetAppointmentsFilteredRequest
        {
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.Subtract(1.Days())
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var request = new GetAppointmentsFilteredRequest
        {
            Type = invalidType
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var request = new GetAppointmentsFilteredRequest
        {
            Status = invalidStatus
        };
        var query = new GetFilteredAppointmentsQuery(request);
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

        var request = new GetAppointmentsFilteredRequest
        {
            PatientId = invalidPatientId
        };
        var query = new GetFilteredAppointmentsQuery(request);
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

        var request = new GetAppointmentsFilteredRequest
        {
            DoctorId = invalidDoctorId
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};
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

        var request = new GetAppointmentsFilteredRequest
        {
            DateStart = dateStart,
            DateEnd = dateEnd
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var types = new List<AppointmentType>
        {
            new() {Id = Guid.NewGuid(), Name = "Type1"},
            new() {Id = Guid.NewGuid(), Name = "Type2"},
            new() {Id = Guid.NewGuid(), Name = "Type2"}
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

        var request = new GetAppointmentsFilteredRequest
        {
            Type = selectedType.Name
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var statuses = new List<AppointmentStatus>
        {
            new() {Id = Guid.NewGuid(), Name = "Status1"},
            new() {Id = Guid.NewGuid(), Name = "Status2"},
            new() {Id = Guid.NewGuid(), Name = "Status2"}
        };
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};
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

        var request = new GetAppointmentsFilteredRequest
        {
            Status = selectedStatus.Name
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patients = new List<Patient>
        {
            new() {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""},
            new() {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""},
            new() {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""}
        };
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};
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

        var request = new GetAppointmentsFilteredRequest
        {
            PatientId = selectedPatient.Id
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var doctors = new List<Doctor>
        {
            new() {Id = Guid.NewGuid()},
            new() {Id = Guid.NewGuid()},
            new() {Id = Guid.NewGuid()}
        };
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};
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

        var request = new GetAppointmentsFilteredRequest
        {
            DoctorId = selectedDoctor.Id
        };
        var query = new GetFilteredAppointmentsQuery(request);
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
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(doctor);
        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(status);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(type);

        var request = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Type = type.Name,
            Description = ""
        };
        var command = new CreateAppointmentCommand(request, status.Name, string.Empty, Guid.NewGuid());
        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.CreateAsync(A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("DoctorId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("PatientId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("Status", "NonExistingStatus")]
    [InlineData("Type", "NonExistingType")]
    public async Task CreateAppointmentHandler_ContainsInvalidField_ThrowsBadRequestException(
        string fieldName, string fieldValue)
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = Guid.NewGuid(), Name = "Pending"};
        var type = new AppointmentType {Id = Guid.NewGuid(), Name = "Consultation"};

        var request = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Type = type.Name,
            Description = ""
        };

        if (fieldName == "Status")
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
                .Throws(new NotFoundException(""));
        else
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(status);

        if (fieldName == "Type")
            A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored))
                .Throws(new NotFoundException(""));
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

        if (fieldName != "Status")
        {
            if (Guid.TryParse(fieldValue, out var guid))
            {
                typeof(CreateAppointmentRequest)
                    .GetProperty(fieldName)!
                    .SetValue(request, guid);
            }
            else
            {
                typeof(CreateAppointmentRequest)
                    .GetProperty(fieldName)!
                    .SetValue(request, fieldValue);
            }
        }

        CreateAppointmentCommand command;
        if (fieldName == "Status")
            command = new CreateAppointmentCommand(request, fieldValue, string.Empty, Guid.NewGuid());
        else
            command = new CreateAppointmentCommand(request, status.Name, string.Empty, Guid.NewGuid());

        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var action = async () => await handler.Handle(command, CancellationToken.None);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_IdAndRequestAreValid_UpdatesAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType {Id = Guid.NewGuid(), Name = "new type"};
        var newAppointmentStatus = new AppointmentStatus {Id = Guid.NewGuid(), Name = "new status"};

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = newAppointmentType.Name,
            Description = "newDescription",
            Status = newAppointmentStatus.Name
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(
            request: request,
            appointmentId: appointment.Id,
            userId: Guid.NewGuid(),
            role: string.Empty
        );

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Appointment>.Ignored))
            .MustHaveHappenedOnceExactly();
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

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = "new type",
            Description = "newDescription",
            Status = "new status"
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(
            request: request,
            appointmentId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            role: string.Empty
        );

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
    public async Task UpdateAppointmentHandler_SingleFieldIsPresent_UpdatesRequestedField(string fieldName,
        string fieldValue)
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var newAppointmentType = new AppointmentType {Id = Guid.NewGuid(), Name = "new type"};
        var newAppointmentStatus = new AppointmentStatus {Id = Guid.NewGuid(), Name = "new status"};

        var request = new UpdateAppointmentRequest();
        if (fieldName == "Date")
            request.Date = DateTime.Parse(fieldValue);
        else
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(request, fieldValue);

        var updateAppointmentCommand = new UpdateAppointmentCommand(
            request: request,
            appointmentId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            role: string.Empty
        );

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Appointment>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_RequestedTypeDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        var appointment = GetAppointments(1)[0];

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = "nonExistingType",
            Description = ""
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(
            request: request,
            appointmentId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            role: string.Empty
        );

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentStatus());
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));

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

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Status = "nonExistingStatus",
            Description = ""
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(
            request: request,
            appointmentId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            role: string.Empty
        );

        A.CallTo(() => _fakeAppointmentRepository.GetByIdAsync(
            A<Guid>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored,
            A<Expression<Func<Appointment, object>>>.Ignored
        )).Returns(appointment);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));
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

        var doctor = new Doctor {Id = doctorId};
        var patient = new Patient {Id = patientId, FirstName = "", LastName = "", Address = ""};
        var status = new AppointmentStatus {Id = appointmentStatusId, Name = "Pending"};
        var type = new AppointmentType {Id = appointmentTypeId, Name = "Consultation"};

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