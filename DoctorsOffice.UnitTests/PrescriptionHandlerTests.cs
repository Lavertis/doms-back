using DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;
using DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionById;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class PrescriptionHandlerTests : UnitTest
{
    private readonly IPrescriptionRepository _fakePrescriptionRepository;

    public PrescriptionHandlerTests()
    {
        _fakePrescriptionRepository = A.Fake<IPrescriptionRepository>();
    }

    [Fact]
    public async Task GetPrescriptionByIdHandler_PrescriptionExists_ReturnsPrescription()
    {
        // arrange
        var prescriptionId = Guid.NewGuid();

        var prescription = new Prescription
        {
            Id = prescriptionId,
            DrugItems = new List<DrugItem> { new() }
        };
        var prescriptionsQueryable = new List<Prescription> { prescription }.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionByIdQuery(prescriptionId);
        var handler = new GetPrescriptionByIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value.Should().BeEquivalentTo(Mapper.Map<PrescriptionResponse>(prescription));
    }

    [Fact]
    public async Task GetPrescriptionByIdHandler_PrescriptionDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var prescriptionsQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionByIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionByIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_PatientExists_ReturnsPrescriptionsBelongingToPatient()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var prescriptionsQueryable = new List<Prescription>
        {
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } },
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } },
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } }
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionsByPatientIdQuery { PatientId = patientId };
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().HaveCount(prescriptionsQueryable.Count());
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_PatientDoesntHavePrescriptions_ReturnsEmptyList()
    {
        // arrange
        var dummyPrescriptionQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(dummyPrescriptionQueryable);

        var query = new GetPrescriptionsByPatientIdQuery { PatientId = Guid.NewGuid() };
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_NoPaginationProvided_ReturnsPrescriptionsBelongingToPatient()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var prescriptionsQueryable = new List<Prescription>
        {
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } },
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } },
            new() { PatientId = patientId, DrugItems = new List<DrugItem> { new() } }
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var expectedResponse = await prescriptionsQueryable
            .Select(p => Mapper.Map<PrescriptionResponse>(p))
            .ToListAsync();

        var query = new GetPrescriptionsByPatientIdQuery { PatientId = patientId };
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_PaginationProvided_ReturnsPrescriptionsBelongingToPatient()
    {
        // arrange
        var patientId = Guid.NewGuid();
        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 20; i++)
        {
            prescriptions.Add(new Prescription { PatientId = patientId, DrugItems = new List<DrugItem> { new() } });
        }

        var prescriptionsQueryable = prescriptions.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        const int pageSize = 5;
        const int pageNumber = 2;

        var expectedResponse = await prescriptionsQueryable
            .Select(p => Mapper.Map<PrescriptionResponse>(p))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var query = new GetPrescriptionsByPatientIdQuery
        {
            PatientId = patientId,
            PaginationFilter = new PaginationFilter { PageSize = pageSize, PageNumber = pageNumber }
        };
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_DoctorExists_ReturnsPrescriptionsBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var prescriptionsQueryable = new List<Prescription>
        {
            new() { DoctorId = doctorId, DrugItems = new List<DrugItem> { new() } },
            new() { DoctorId = doctorId, DrugItems = new List<DrugItem> { new() } },
            new() { DoctorId = doctorId, DrugItems = new List<DrugItem> { new() } }
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionsByDoctorIdQuery { DoctorId = doctorId };
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().HaveCount(prescriptionsQueryable.Count());
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_DoctorDoesntHavePrescriptions_ReturnsEmptyList()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var dummyPrescriptionQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(dummyPrescriptionQueryable);

        var query = new GetPrescriptionsByDoctorIdQuery { DoctorId = doctorId };
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_NoPaginationProvided_ReturnsPrescriptionsBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 20; i++)
        {
            prescriptions.Add(new Prescription { DoctorId = doctorId, DrugItems = new List<DrugItem> { new() } });
        }

        var prescriptionsQueryable = prescriptions.AsQueryable().BuildMock();

        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var expectedResponse = await prescriptionsQueryable
            .Select(p => Mapper.Map<PrescriptionResponse>(p))
            .ToListAsync();

        var query = new GetPrescriptionsByDoctorIdQuery { DoctorId = doctorId };
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_PaginationProvided_ReturnsPrescriptionsBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var prescriptions = new List<Prescription>();
        for (var i = 0; i < 20; i++)
        {
            prescriptions.Add(new Prescription { DoctorId = doctorId, DrugItems = new List<DrugItem> { new() } });
        }

        var prescriptionsQueryable = prescriptions.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        const int pageSize = 5;
        const int pageNumber = 2;

        var expectedResponse = await prescriptionsQueryable
            .Select(p => Mapper.Map<PrescriptionResponse>(p))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var query = new GetPrescriptionsByDoctorIdQuery
        {
            DoctorId = doctorId,
            PaginationFilter = new PaginationFilter { PageSize = pageSize, PageNumber = pageNumber }
        };
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CreatePrescriptionHandler_ValidRequest_CreatesPrescription()
    {
        // arrange
        var expectedPrescription = new Prescription
        {
            Title = "Test Prescription",
            Description = "Test Description",
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DrugItems = new List<DrugItem> { new(), new() }
        };
        A.CallTo(() => _fakePrescriptionRepository.CreateAsync(A<Prescription>.Ignored))
            .Returns(expectedPrescription);

        var request = new CreatePrescriptionRequest
        {
            Title = expectedPrescription.Title,
            Description = expectedPrescription.Description,
            PatientId = expectedPrescription.PatientId,
            DrugsIds = expectedPrescription.DrugItems.Select(d => d.Id).ToList()
        };
        var command = new CreatePrescriptionCommand(request, expectedPrescription.DoctorId);
        var handler = new CreatePrescriptionHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePrescriptionRepository.CreateAsync(A<Prescription>.Ignored))
            .MustHaveHappenedOnceExactly();

        result.Value.Should().BeEquivalentTo(Mapper.Map<PrescriptionResponse>(expectedPrescription));
    }

    [Fact]
    public async Task UpdatePrescriptionHandler_ValidRequest_UpdatesPrescription()
    {
        // arrange
        var prescriptionId = Guid.NewGuid();
        var oldPrescription = new Prescription
        {
            Id = prescriptionId,
            Title = "Old Prescription Title",
            Description = "Old Description",
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DrugItems = new List<DrugItem> { new(), new() }
        };

        var expectedPrescription = new Prescription
        {
            Id = prescriptionId,
            Title = "New Prescription Title",
            Description = "New Description",
            PatientId = Guid.NewGuid(),
            DoctorId = oldPrescription.DoctorId,
            DrugItems = new List<DrugItem> { new(), new() }
        };

        A.CallTo(() => _fakePrescriptionRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(oldPrescription);
        var prescriptionsQueryable = new List<Prescription> { oldPrescription }.AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);
        A.CallTo(() => _fakePrescriptionRepository.UpdateAsync(A<Prescription>.Ignored))
            .Returns(oldPrescription);

        var request = new UpdatePrescriptionRequest
        {
            Title = expectedPrescription.Title,
            Description = expectedPrescription.Description,
            PatientId = expectedPrescription.PatientId,
            DrugIds = expectedPrescription.DrugItems.Select(d => d.Id).ToList()
        };
        var command = new UpdatePrescriptionCommand(request, prescriptionId);
        var handler = new UpdatePrescriptionHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePrescriptionRepository.UpdateAsync(A<Prescription>.Ignored))
            .MustHaveHappenedOnceExactly();
        result.Value.Should().BeEquivalentTo(Mapper.Map<PrescriptionResponse>(expectedPrescription));
    }

    [Fact]
    public async Task UpdatePrescriptionHandler_PrescriptionWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var prescriptionsQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakePrescriptionRepository.GetAll()).Returns(prescriptionsQueryable);

        var request = new UpdatePrescriptionRequest
        {
            Title = "New Prescription Title",
            Description = "New Description",
            PatientId = Guid.NewGuid(),
            DrugIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };
        var command = new UpdatePrescriptionCommand(request, Guid.NewGuid());
        var handler = new UpdatePrescriptionHandler(_fakePrescriptionRepository, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}