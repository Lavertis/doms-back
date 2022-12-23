using DoctorsOffice.Application.CQRS.Queries.Timetables.GetTimetablesByDoctorId;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class TimetableHandlerTests : UnitTest
{
    private readonly ITimetableRepository _fakeTimetableRepository;

    public TimetableHandlerTests()
    {
        _fakeTimetableRepository = A.Fake<ITimetableRepository>();
    }

    [Fact]
    public async Task GetTimetablesByDoctorIdHandler_DoctorIdExists_ReturnsAllTimetablesBelongingToDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var timetables = GetTimetables(10);
        timetables.AddRange(GetTimetables(10, doctorId));
        var timetablesQueryable = timetables.BuildMock();

        A.CallTo(() => _fakeTimetableRepository.GetAll()).Returns(timetablesQueryable);

        var query = new GetTimetablesByDoctorIdQuery
        {
            DoctorId = doctorId
        };
        var handler = new GetTimetablesByDoctorIdHandler(_fakeTimetableRepository, Mapper);

        var expectedResponse = timetables
            .Where(t => t.DoctorId == doctorId)
            .Select(t => Mapper.Map<TimetableResponse>(t));

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetTimetablesByDoctorIdHandler_FiltersProvided_ReturnsAllAppointmentsMatchingConstraints()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var timetables = GetTimetables(10, doctorId, DateTime.UtcNow);
        timetables.AddRange(GetTimetables(10, doctorId, DateTime.UtcNow.Add(10.Days())));
        var timetablesQueryable = timetables.BuildMock();

        A.CallTo(() => _fakeTimetableRepository.GetAll()).Returns(timetablesQueryable);

        var query = new GetTimetablesByDoctorIdQuery
        {
            DoctorId = doctorId,
            StartDateTime = DateTime.UtcNow.Subtract(1.Hours()),
            EndDateTime = DateTime.UtcNow.Add(1.Hours())
        };
        var handler = new GetTimetablesByDoctorIdHandler(_fakeTimetableRepository, Mapper);

        var expectedResponse = timetables
            .Where(t => t.DoctorId == doctorId)
            .Where(t => t.StartDateTime >= query.StartDateTime)
            .Where(t => t.EndDateTime <= query.EndDateTime)
            .Select(t => Mapper.Map<TimetableResponse>(t));

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetTimetablesByDoctorIdHandler_DoctorWithSpecifiedIdDoesntHaveTimetables_ReturnsEmptyList()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var timetablesQueryable = GetTimetables(10).BuildMock();

        A.CallTo(() => _fakeTimetableRepository.GetAll()).Returns(timetablesQueryable);

        var query = new GetTimetablesByDoctorIdQuery
        {
            DoctorId = doctorId
        };
        var handler = new GetTimetablesByDoctorIdHandler(_fakeTimetableRepository, Mapper);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        result.Value.Should().BeEmpty();
    }

    // TODO write tests for UpdateTimetablesHandler
    // TODO write tests for DeleteTimetablesHandler

    private static List<Timetable> GetTimetables(int count, Guid? doctorId = null, DateTime? baseDate = null)
    {
        var timetables = new List<Timetable>();

        baseDate ??= DateTime.UtcNow;

        for (var i = 0; i < count; i++)
        {
            timetables.Add(new Timetable
            {
                DoctorId = doctorId ?? Guid.NewGuid(),
                StartDateTime = baseDate.Value.Add((5 * i).Hours()),
                EndDateTime = baseDate.Value.Add((5 * i + 4).Hours())
            });
        }

        return timetables;
    }
}