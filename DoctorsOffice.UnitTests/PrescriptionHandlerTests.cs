using System.Linq.Expressions;
using DoctorsOffice.Application.CQRS.Commands.CreatePrescription;
using DoctorsOffice.Application.CQRS.Commands.UpdatePrescription;
using DoctorsOffice.Application.CQRS.Queries.GetPrescriptionById;
using DoctorsOffice.Application.CQRS.Queries.GetPrescriptionsByDoctorId;
using DoctorsOffice.Application.CQRS.Queries.GetPrescriptionsByPatientId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class PrescriptionHandlerTests
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
            DrugItems = new List<DrugItem> {new()}
        };

        A.CallTo(() => _fakePrescriptionRepository.GetByIdAsync(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Returns(prescription);

        var query = new GetPrescriptionByIdQuery(prescriptionId);
        var handler = new GetPrescriptionByIdHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEquivalentTo(new PrescriptionResponse(prescription));
    }

    [Fact]
    public async Task GetPrescriptionByIdHandler_PrescriptionDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakePrescriptionRepository.GetByIdAsync(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Throws(new NotFoundException(""));

        var query = new GetPrescriptionByIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionByIdHandler(_fakePrescriptionRepository);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_PatientExists_ReturnsPrescriptionsBelongingToPatient()
    {
        // arrange
        var prescriptionsQueryable = new List<Prescription>
        {
            new() {DrugItems = new List<DrugItem> {new()}},
            new() {DrugItems = new List<DrugItem> {new()}},
            new() {DrugItems = new List<DrugItem> {new()}}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakePrescriptionRepository.GetByPatientId(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionsByPatientIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().HaveCount(prescriptionsQueryable.Count());
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdHandler_PatientDoesntHavePrescriptions_ReturnsEmptyList()
    {
        // arrange
        var dummyPrescriptionQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();

        A.CallTo(() => _fakePrescriptionRepository.GetByPatientId(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Returns(dummyPrescriptionQueryable);

        var query = new GetPrescriptionsByPatientIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionsByPatientIdHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_DoctorExists_ReturnsPrescriptionsBelongingToDoctor()
    {
        // arrange
        var prescriptionsQueryable = new List<Prescription>
        {
            new() {DrugItems = new List<DrugItem> {new()}},
            new() {DrugItems = new List<DrugItem> {new()}},
            new() {DrugItems = new List<DrugItem> {new()}}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakePrescriptionRepository.GetByDoctorId(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Returns(prescriptionsQueryable);

        var query = new GetPrescriptionsByDoctorIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().HaveCount(prescriptionsQueryable.Count());
    }

    [Fact]
    public async Task GetPrescriptionsByDoctorIdHandler_DoctorDoesntHavePrescriptions_ReturnsEmptyList()
    {
        // arrange
        var dummyPrescriptionQueryable = A.CollectionOfDummy<Prescription>(0).AsQueryable().BuildMock();

        A.CallTo(() => _fakePrescriptionRepository.GetByDoctorId(
            A<Guid>.Ignored, A<Expression<Func<Prescription, object>>>.Ignored
        )).Returns(dummyPrescriptionQueryable);

        var query = new GetPrescriptionsByDoctorIdQuery(Guid.NewGuid());
        var handler = new GetPrescriptionsByDoctorIdHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePrescription_ValidRequest_CreatesPrescription()
    {
        // arrange
        var expectedPrescription = new Prescription()
        {
            Title = "Test Prescription",
            Description = "Test Description",
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DrugItems = new List<DrugItem> {new(), new()}
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
        var handler = new CreatePrescriptionHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePrescriptionRepository.CreateAsync(A<Prescription>.Ignored))
            .MustHaveHappenedOnceExactly();

        result.Should().BeEquivalentTo(new PrescriptionResponse(expectedPrescription));
    }

    [Fact]
    public async Task UpdatePrescription_ValidRequest_UpdatesPrescription()
    {
        // arrange
        var oldPrescription = new Prescription
        {
            Title = "Old Prescription Title",
            Description = "Old Description",
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DrugItems = new List<DrugItem> {new(), new()}
        };

        var expectedPrescription = new Prescription
        {
            Title = "New Prescription Title",
            Description = "New Description",
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DrugItems = new List<DrugItem> {new(), new()}
        };

        A.CallTo(() => _fakePrescriptionRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(oldPrescription);
        A.CallTo(() =>
                _fakePrescriptionRepository.GetByIdAsync(A<Guid>.Ignored,
                    A<Expression<Func<Prescription, object>>>.Ignored))
            .Returns(expectedPrescription);
        A.CallTo(() => _fakePrescriptionRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Prescription>.Ignored))
            .Returns(oldPrescription);

        var request = new UpdatePrescriptionRequest
        {
            Title = expectedPrescription.Title,
            Description = expectedPrescription.Description,
            PatientId = expectedPrescription.PatientId,
            DrugsIds = expectedPrescription.DrugItems.Select(d => d.Id).ToList()
        };
        var command = new UpdatePrescriptionCommand(request, Guid.NewGuid());
        var handler = new UpdatePrescriptionHandler(_fakePrescriptionRepository);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePrescriptionRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Prescription>.Ignored))
            .MustHaveHappenedOnceExactly();
        result.Should().BeEquivalentTo(new PrescriptionResponse(expectedPrescription));
    }

    [Fact]
    public async Task UpdatePrescription_PrescriptionWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() =>
                _fakePrescriptionRepository.GetByIdAsync(A<Guid>.Ignored,
                    A<Expression<Func<Prescription, object>>>.Ignored))
            .Throws(new NotFoundException(""));

        var request = new UpdatePrescriptionRequest
        {
            Title = "New Prescription Title",
            Description = "New Description",
            PatientId = Guid.NewGuid(),
            DrugsIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}
        };
        var command = new UpdatePrescriptionCommand(request, Guid.NewGuid());
        var handler = new UpdatePrescriptionHandler(_fakePrescriptionRepository);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}