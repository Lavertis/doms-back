using DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;
using DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AdminHandlerTests : UnitTest
{
    private readonly IAdminRepository _fakeAdminRepository;

    public AdminHandlerTests()
    {
        _fakeAdminRepository = A.Fake<IAdminRepository>();
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminExists_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var adminsQueryable = new List<Admin>
        {
            new()
            {
                Id = adminId,
                AppUser = new AppUser {Id = adminId}
            }
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll())
            .Returns(adminsQueryable);

        var expectedResponse = new AdminResponse {Id = adminId};

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var adminsQueryable = new List<Admin>().AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll()).Returns(adminsQueryable);

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreAdmins_ReturnsAllAdmins()
    {
        // arrange
        var adminsQueryable = new List<Admin>
        {
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAdminRepository.GetAll()).Returns(adminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var admin in adminsQueryable)
            result.Value!.Records.Should().ContainEquivalentOf(new AdminResponse {Id = admin.Id});
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreNoAdmins_ReturnsEmptyList()
    {
        // arrange
        var dummyAdminsQueryable = A.CollectionOfDummy<Admin>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll()).Returns(dummyAdminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAdminsHandler_NoPageSizeAndPageNumberProvided_ReturnsAllAdmins()
    {
        // arrange
        var adminsQueryable = new List<Admin>
        {
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll()).Returns(adminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var admin in adminsQueryable)
            result.Value!.Records.Should().ContainEquivalentOf(new AdminResponse {Id = admin.Id});
    }

    [Fact]
    public async Task GetAllAdminsHandler_PageSizeAndPageNumberProvided_ReturnsSpecifiedPage()
    {
        // arrange
        var adminsQueryable = new List<Admin>
        {
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}},
            new() {AppUser = new AppUser {Id = Guid.NewGuid()}}
        }.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll()).Returns(adminsQueryable);

        const int pageSize = 2;
        const int pageNumber = 2;

        var expectedResponse = adminsQueryable
            .Select(a => Mapper.Map<AdminResponse>(a))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var query = new GetAllAdminsQuery
        {
            PaginationFilter = new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber}
        };
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }
}