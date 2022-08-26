using DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;
using DoctorsOffice.Application.CQRS.Commands.Appointments.UpdateAppointment;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;
using DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;
using DoctorsOffice.Application.Services.Appointments;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AppointmentHandlerTests : UnitTest
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
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);

        var expectedResponse = appointmentsQueryable.Select(a => Mapper.Map<AppointmentResponse>(a));

        var query = new GetAppointmentsByUserQuery {UserId = patientId, RoleName = RoleTypes.Patient};
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdHandler_PatientDoesntExist_ReturnsEmptyList()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var emptyAppointmentQueryable = A.CollectionOfDummy<Appointment>(0).BuildMock();
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(emptyAppointmentQueryable);

        var query = new GetAppointmentsByUserQuery {UserId = patientId, RoleName = RoleTypes.Patient};
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorExists_ReturnsALlAppointments()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var appointmentsQueryable = GetAppointments(3, patientId: Guid.NewGuid(), doctorId: doctorId).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);

        var expectedResponse = appointmentsQueryable.Select(a => Mapper.Map<AppointmentResponse>(a));

        var query = new GetAppointmentsByUserQuery {UserId = doctorId, RoleName = RoleTypes.Doctor};
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdHandler_DoctorDoesntExist_ReturnsEmptyList()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        var emptyAppointmentQueryable = A.CollectionOfDummy<Appointment>(0).BuildMock();

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(emptyAppointmentQueryable);

        var query = new GetAppointmentsByUserQuery {UserId = doctorId, RoleName = RoleTypes.Doctor};
        var handler = new GetAppointmentsByUserHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentByIdHandler_AppointmentExists_ReturnsAppointment()
    {
        // arrange
        var appointment = GetAppointments(1).First();
        var appointmentsQueryable = new List<Appointment> {appointment}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);
        A.CallTo(() => _fakeAppointmentService.CanUserAccessAppointment(
            A<Guid>.Ignored,
            A<string>.Ignored,
            A<Guid>.Ignored,
            A<Guid>.Ignored
        )).Returns(new CommonResult<bool>().WithValue(true));

        var expectedResponse = Mapper.Map<AppointmentResponse>(appointment);

        var query = new GetAppointmentByIdQuery
        {
            AppointmentId = appointment.Id,
            UserId = Guid.NewGuid(),
            RoleName = RoleTypes.Admin
        };
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository, _fakeAppointmentService, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAppointmentByIdHandler_AppointmentDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var nonExistingAppointmentId = Guid.NewGuid();
        var appointmentsQueryable = A.CollectionOfDummy<Appointment>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);

        var query = new GetAppointmentByIdQuery
        {
            AppointmentId = nonExistingAppointmentId,
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };
        var handler = new GetAppointmentByIdHandler(_fakeAppointmentRepository, _fakeAppointmentService, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_EveryFilterIsNull_ReturnsAllAppointments()
    {
        // arrange
        var appointments = GetAppointments(3);

        var expectedResponse = appointments.Select(a => Mapper.Map<AppointmentSearchResponse>(a));

        var query = new GetFilteredAppointmentsQuery(new PaginationFilter());
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
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

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            DateStart = DateTime.UtcNow.AddDays(1),
            DateEnd = DateTime.UtcNow.Subtract(1.Days())
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidType_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        const string invalidType = "InvalidType";
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            Type = invalidType
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidStatus_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        const string invalidStatus = "InvalidStatus";
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            Status = invalidStatus
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidPatientId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var invalidPatientId = Guid.NewGuid();
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            PatientId = invalidPatientId
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_InvalidDoctorId_ReturnsEmptyList()
    {
        // arrange
        var appointments = GetAppointments(3);
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var invalidDoctorId = Guid.NewGuid();
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            DoctorId = invalidDoctorId
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidDateRange_ReturnsAppointmentsInDateRange()
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Email = "", PhoneNumber = ""}
        };
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

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var dateStart = DateTime.UtcNow.AddDays(1);
        var dateEnd = DateTime.UtcNow.AddDays(2);

        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            DateStart = dateStart,
            DateEnd = dateEnd
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().NotBeEmpty();
        result.Value!.Records.Should().OnlyContain(a => a.Date >= dateStart && a.Date <= dateEnd);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidType_ReturnsAppointmentsMatchingType()
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Email = "", PhoneNumber = ""}
        };
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

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            Type = selectedType.Name
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().NotBeEmpty();
        result.Value!.Records.Should().OnlyContain(a => a.Type == selectedType.Name);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidStatus_ReturnsAppointmentsMatchingStatus()
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Email = "", PhoneNumber = ""}
        };
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

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var selectedStatus = statuses[1];
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            Status = selectedStatus.Name
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().NotBeEmpty();
        result.Value!.Records.Should().OnlyContain(a => a.Status == selectedStatus.Name);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidPatientId_ReturnsAppointmentsBelongingToPatient()
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patients = new List<Patient>
        {
            new()
            {
                Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "",
                AppUser = new AppUser {Email = "", PhoneNumber = ""}
            },
            new()
            {
                Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "",
                AppUser = new AppUser {Email = "", PhoneNumber = ""}
            },
            new()
            {
                Id = Guid.NewGuid(), FirstName = "", LastName = "", Address = "",
                AppUser = new AppUser {Email = "", PhoneNumber = ""}
            }
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

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var selectedPatient = patients[1];
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            PatientId = selectedPatient.Id
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().NotBeEmpty();
        result.Value!.Records.Should().OnlyContain(a => a.PatientId == selectedPatient.Id);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_ValidDoctorId_ReturnsAppointmentsBelongingToDoctor()
    {
        // arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Email = "", PhoneNumber = ""}
        };
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

        var doctorsAppointments = appointments.Where(a => a.DoctorId == doctors[1].Id).ToList();

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        var selectedDoctor = doctors[1];
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter())
        {
            DoctorId = selectedDoctor.Id
        };
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().NotBeEmpty();
        result.Value!.Records.Should().OnlyContain(appointmentResponse =>
            doctorsAppointments.Any(a => a.Id == appointmentResponse.Id));
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_NoPageSizeAndPageNumberProvided_ReturnsAllAppointments()
    {
        // arrange
        var appointments = GetAppointments(3);
        var expectedResponse = appointments.Select(a => Mapper.Map<AppointmentSearchResponse>(a));
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter());
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_OnlyPageSizeIsProvided_ReturnsBadRequest400StatusCode()
    {
        // arrange
        const int pageSize = 10;
        var appointments = GetAppointments(35);
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter {PageSize = pageSize});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_OnlyPageNumberIsProvided_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var appointments = GetAppointments(35);
        var query = new GetFilteredAppointmentsQuery(new PaginationFilter {PageNumber = 2});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_PageSizeIsNegative_ReturnsBadRequest400StatusCode()
    {
        // arrange
        const int pageSize = -10;
        const int pageNumber = 2;
        var appointments = GetAppointments(35);
        var query = new GetFilteredAppointmentsQuery(
            new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetFilteredAppointmentsHandler_PageNumberIsNegative_ReturnsBadRequest400StatusCode()
    {
        // arrange
        const int pageSize = 10;
        const int pageNumber = -2;
        var appointments = GetAppointments(35);

        var query = new GetFilteredAppointmentsQuery(
            new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task
        GetFilteredAppointmentsHandler_PageSizeIsHigherThanNumberOfRecords_ResultPageSizeIsEqualToNumberOfRecords()
    {
        // arrange
        const int pageSize = 45;
        const int pageNumber = 1;
        var appointments = GetAppointments(pageSize - 10);

        const int expectedPageNumber = 1;
        var expectedPageSize = appointments.Count;
        var expectedResponse = appointments
            .Select(a => Mapper.Map<AppointmentSearchResponse>(a))
            .Skip((expectedPageNumber - 1) * expectedPageSize)
            .Take(expectedPageSize)
            .ToList();

        var query = new GetFilteredAppointmentsQuery(
            new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
        result.Value!.PageSize.Should().Be(expectedPageSize);
        result.Value!.PageNumber.Should().Be(expectedPageNumber);
    }

    [Fact]
    public async Task
        GetFilteredAppointmentsHandler_PageNumberIsHigherThanNumberOfRecords_ReturnsBadRequest400StatusCode()
    {
        // arrange
        const int pageSize = 5;
        const int pageNumber = 20;
        var appointments = GetAppointments(pageNumber - 3);
        var query = new GetFilteredAppointmentsQuery(
            new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber});
        var handler = new GetFilteredAppointmentsHandler(_fakeAppointmentRepository, Mapper);

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointments.AsQueryable().BuildMock());

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
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
        var appointmentsQueryable = A.CollectionOfDummy<Appointment>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);

        var request = new CreateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            Type = type.Name,
            Description = ""
        };
        var command = new CreateAppointmentCommand(request)
        {
            Status = status.Name,
            RoleName = string.Empty,
            UserId = Guid.NewGuid()
        };
        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        await handler.Handle(command, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.CreateAsync(A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("DoctorId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("PatientId", "f99d9ea4-333f-4f19-affd-7a8886188ce8")]
    [InlineData("Status", "NonExistingStatus")]
    [InlineData("Type", "NonExistingType")]
    public async Task CreateAppointmentHandler_ContainsNonExistingFieldValue_ReturnsNotFound404StatusCode(
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
        {
            AppointmentStatus? nullStatus = null;
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(nullStatus);
        }
        else
            A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(status);

        if (fieldName == "Type")
        {
            AppointmentType? nullType = null;
            A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(nullType);
        }
        else
            A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(type);

        if (fieldName == "PatientId")
        {
            Patient? nullPatient = null;
            A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(nullPatient);
        }
        else
            A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);

        if (fieldName == "DoctorId")
        {
            Doctor? nullDoctor = null;
            A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(nullDoctor);
        }
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
        {
            command = new CreateAppointmentCommand(request)
            {
                UserId = Guid.NewGuid(),
                Status = fieldValue,
                RoleName = string.Empty
            };
        }
        else
        {
            command = new CreateAppointmentCommand(request)
            {
                UserId = Guid.NewGuid(),
                Status = status.Name,
                RoleName = string.Empty
            };
        }

        var handler = new CreateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAppointmentHandler_IdAndRequestAreValid_UpdatesAppointment()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var appointmentsQueryable = new List<Appointment> {appointment}.AsQueryable().BuildMock();
        var newAppointmentType = new AppointmentType {Id = Guid.NewGuid(), Name = "new type"};
        var newAppointmentStatus = new AppointmentStatus {Id = Guid.NewGuid(), Name = "new status"};

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);
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
        var updateAppointmentCommand = new UpdateAppointmentCommand(request)
        {
            AppointmentId = appointment.Id,
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateAsync(A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_IdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var appointmentsQueryable = A.CollectionOfDummy<Appointment>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = "new type",
            Description = "newDescription",
            Status = "new status"
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(request)
        {
            AppointmentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
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
        var appointmentsQueryable = new List<Appointment> {appointment}.AsQueryable().BuildMock();
        var newAppointmentType = new AppointmentType {Id = Guid.NewGuid(), Name = "new type"};
        var newAppointmentStatus = new AppointmentStatus {Id = Guid.NewGuid(), Name = "new status"};

        var request = new UpdateAppointmentRequest();
        if (fieldName == "Date")
            request.Date = DateTime.Parse(fieldValue);
        else
            typeof(UpdateAppointmentRequest).GetProperty(fieldName)!.SetValue(request, fieldValue);

        var updateAppointmentCommand = new UpdateAppointmentCommand(request)
        {
            AppointmentId = appointment.Id,
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Returns(newAppointmentStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(newAppointmentType);

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        A.CallTo(() => _fakeAppointmentRepository.UpdateAsync(A<Appointment>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAppointmentHandler_RequestedTypeDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var appointmentsQueryable = new List<Appointment> {appointment}.AsQueryable().BuildMock();

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Type = "nonExistingType",
            Description = ""
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(request)
        {
            AppointmentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored))
            .Returns(new AppointmentStatus());
        AppointmentType? nullType = null;
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(nullType);

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAppointmentHandler_RequestedStatusDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var appointment = GetAppointments(1)[0];
        var appointmentsQueryable = new List<Appointment> {appointment}.AsQueryable().BuildMock();

        var request = new UpdateAppointmentRequest
        {
            Date = DateTime.UtcNow.AddDays(100),
            Status = "nonExistingStatus",
            Description = ""
        };
        var updateAppointmentCommand = new UpdateAppointmentCommand(request)
        {
            AppointmentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoleName = string.Empty
        };

        A.CallTo(() => _fakeAppointmentRepository.GetAll()).Returns(appointmentsQueryable);
        AppointmentStatus? nullStatus = null;
        A.CallTo(() => _fakeAppointmentStatusRepository.GetByNameAsync(A<string>.Ignored)).Returns(nullStatus);
        A.CallTo(() => _fakeAppointmentTypeRepository.GetByNameAsync(A<string>.Ignored)).Returns(new AppointmentType());

        var handler = new UpdateAppointmentHandler(
            _fakeAppointmentRepository,
            _fakeAppointmentStatusRepository,
            _fakeAppointmentTypeRepository,
            Mapper
        );

        // act
        var result = await handler.Handle(updateAppointmentCommand, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
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
        var patient = new Patient
        {
            Id = patientId,
            FirstName = "",
            LastName = "",
            Address = "",
            AppUser = new AppUser {Email = "", PhoneNumber = ""}
        };
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