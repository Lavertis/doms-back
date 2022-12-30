using DoctorsOffice.Application.CQRS.Commands.SickLeaves.CreateSickLeave;
using DoctorsOffice.Application.CQRS.Commands.SickLeaves.UpdateSickLeave;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeavesByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetSickLeavesByPatientId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class SickLeaveHandlerTests : UnitTest
{
    private readonly IDoctorRepository _fakeDoctorRepository;
    private readonly IPatientRepository _fakePatientRepository;
    private readonly ISickLeaveRepository _fakeSickLeaveRepository;

    public SickLeaveHandlerTests()
    {
        _fakeSickLeaveRepository = A.Fake<ISickLeaveRepository>();
        _fakePatientRepository = A.Fake<IPatientRepository>();
        _fakeDoctorRepository = A.Fake<IDoctorRepository>();
    }

    [Fact]
    public async Task GetSickLeavesByPatientIdHandler_PatientDoesntHaveSickLeaves_ReturnsEmptyList()
    {
        // arrange
        var dummySickLeaveQueryable = A.CollectionOfDummy<SickLeave>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(dummySickLeaveQueryable);

        var query = new GetSickLeavesByPatientIdQuery() {PatientId = Guid.NewGuid()};
        var handler = new GetSickLeavesByPatientIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSickLeavesByPatientIdHandler_NoPaginationProvided_ReturnsSickLeavesBelongingToPatient()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var sickLeavesQueryable = new List<SickLeave>
        {
            new() {PatientId = patientId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
            new() {PatientId = patientId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
            new() {PatientId = patientId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);

        var expectedResponse = await sickLeavesQueryable
            .Select(s => Mapper.Map<SickLeaveResponse>(s))
            .ToListAsync();

        var query = new GetSickLeavesByPatientIdQuery() {PatientId = patientId};
        var handler = new GetSickLeavesByPatientIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetSickLeavesByPatientIdHandler_PaginationProvided_ReturnsSickLeavesBelongingToPatient()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 20; i++)
        {
            sickLeaves.Add(new SickLeave
                {PatientId = patientId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)});
        }

        var sickLeavesQueryable = sickLeaves.AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);

        const int pageSize = 5;
        const int pageNumber = 2;

        var expectedResponse = await sickLeavesQueryable
            .Select(s => Mapper.Map<SickLeaveResponse>(s))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var query = new GetSickLeavesByPatientIdQuery
        {
            PatientId = patientId,
            PaginationFilter = new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber}
        };
        var handler = new GetSickLeavesByPatientIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetSickLeavesByDoctorIdHandler_DoctorExists_ReturnsSickLeavesBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var sickLeavesQueryable = new List<SickLeave>
        {
            new() {DoctorId = doctorId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
            new() {DoctorId = doctorId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
            new() {DoctorId = doctorId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)},
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);

        var query = new GetSickLeavesByDoctorIdQuery() {DoctorId = doctorId};
        var handler = new GetSickLeavesByDoctorIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().HaveCount(sickLeavesQueryable.Count());
    }

    [Fact]
    public async Task GetSickLeavesByDoctorIdHandler_DoctorDoesntHaveSickLeaves_ReturnsEmptyList()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var dummySickLeaveQueryable = A.CollectionOfDummy<SickLeave>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(dummySickLeaveQueryable);

        var query = new GetSickLeavesByDoctorIdQuery() {DoctorId = doctorId};
        var handler = new GetSickLeavesByDoctorIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSickLeavesByDoctorIdHandler_NoPaginationProvided_ReturnsSickLeavesBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 20; i++)
        {
            sickLeaves.Add(new SickLeave
                {DoctorId = doctorId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)});
        }

        var sickLeavesQueryable = sickLeaves.AsQueryable().BuildMock();

        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);

        var expectedResponse = await sickLeavesQueryable
            .Select(s => Mapper.Map<SickLeaveResponse>(s))
            .ToListAsync();

        var query = new GetSickLeavesByDoctorIdQuery() {DoctorId = doctorId};
        var handler = new GetSickLeavesByDoctorIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetSickLeavesByDoctorIdHandler_PaginationProvided_ReturnsSickLeavesBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var sickLeaves = new List<SickLeave>();
        for (var i = 0; i < 20; i++)
        {
            sickLeaves.Add(new SickLeave
                {DoctorId = doctorId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddDays(2)});
        }

        var sickLeavesQueryable = sickLeaves.AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);

        const int pageSize = 5;
        const int pageNumber = 2;

        var expectedResponse = await sickLeavesQueryable
            .Select(p => Mapper.Map<SickLeaveResponse>(p))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var query = new GetSickLeavesByDoctorIdQuery
        {
            DoctorId = doctorId,
            PaginationFilter = new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber}
        };
        var handler = new GetSickLeavesByDoctorIdHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CreateSickLeaveHandler_ValidRequest_CreatesSickLeave()
    {
        // arrange
        var doctor = new Doctor {Id = Guid.NewGuid()};
        var patient = new Patient {Id = Guid.NewGuid()};
        var expectedSickLeave = new SickLeave
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            AppointmentId = Guid.NewGuid(),
            DateStart = DateTime.UtcNow.AddDays(2),
            DateEnd = DateTime.UtcNow.AddDays(12),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };
        A.CallTo(() => _fakeSickLeaveRepository.CreateAsync(A<SickLeave>.Ignored))
            .Returns(expectedSickLeave);
        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(doctor);
        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);

        var request = new CreateSickLeaveRequest
        {
            PatientId = expectedSickLeave.PatientId,
            AppointmentId = expectedSickLeave.AppointmentId,
            DateStart = expectedSickLeave.DateStart,
            DateEnd = expectedSickLeave.DateEnd,
            Diagnosis = expectedSickLeave.Diagnosis,
            Purpose = expectedSickLeave.Purpose
        };
        var command = new CreateSickLeaveCommand(request, expectedSickLeave.DoctorId);
        var handler = new CreateSickLeaveHandler(
            _fakeSickLeaveRepository,
            _fakeDoctorRepository,
            _fakePatientRepository,
            Mapper
        );

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeSickLeaveRepository.CreateAsync(A<SickLeave>.Ignored))
            .MustHaveHappenedOnceExactly();

        result.Value.Should().BeEquivalentTo(Mapper.Map<SickLeaveResponse>(expectedSickLeave));
    }

    [Fact]
    public async Task UpdateSickLeaveHandler_ValidRequest_UpdatesSickLeave()
    {
        // arrange
        var sickLeaveId = Guid.NewGuid();
        var oldSickLeave = new SickLeave
        {
            Id = sickLeaveId,
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            DateStart = DateTime.UtcNow.AddDays(2),
            DateEnd = DateTime.UtcNow.AddDays(12),
            Diagnosis = "Diagnosis",
            Purpose = "Purpose"
        };

        var expectedSickLeave = new SickLeave
        {
            Id = sickLeaveId,
            PatientId = Guid.NewGuid(),
            DoctorId = oldSickLeave.DoctorId,
            AppointmentId = oldSickLeave.AppointmentId,
            DateStart = DateTime.UtcNow.AddDays(5),
            DateEnd = DateTime.UtcNow.AddDays(14),
            Diagnosis = "Diagnosis2",
            Purpose = "Purpose2"
        };

        A.CallTo(() => _fakeSickLeaveRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(oldSickLeave);
        var sickLeavesQueryable = new List<SickLeave> {oldSickLeave}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeavesQueryable);
        A.CallTo(() => _fakeSickLeaveRepository.UpdateAsync(A<SickLeave>.Ignored))
            .Returns(expectedSickLeave);

        var request = new UpdateSickLeaveRequest
        {
            PatientId = expectedSickLeave.PatientId,
            DoctorId = expectedSickLeave.DoctorId,
            AppointmentId = expectedSickLeave.AppointmentId,
            DateStart = expectedSickLeave.DateStart,
            DateEnd = expectedSickLeave.DateEnd,
            Diagnosis = expectedSickLeave.Diagnosis,
            Purpose = expectedSickLeave.Purpose
        };
        var command = new UpdateSickLeaveCommand(request)
        {
            SickLeaveId = sickLeaveId,
        };
        var handler = new UpdateSickLeaveHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeSickLeaveRepository.UpdateAsync(A<SickLeave>.Ignored))
            .MustHaveHappenedOnceExactly();
        result.Value.Should().BeEquivalentTo(Mapper.Map<SickLeaveResponse>(expectedSickLeave));
    }

    [Fact]
    public async Task UpdateSickLeaveHandler_SickLeaveWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var sickLeaveQueryable = A.CollectionOfDummy<SickLeave>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeSickLeaveRepository.GetAll()).Returns(sickLeaveQueryable);

        var request = new UpdateSickLeaveRequest
        {
            PatientId = new Guid(),
            DoctorId = new Guid(),
            AppointmentId = new Guid(),
        };
        var command = new UpdateSickLeaveCommand(request)
        {
            SickLeaveId = Guid.NewGuid()
        };
        var handler = new UpdateSickLeaveHandler(_fakeSickLeaveRepository, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}