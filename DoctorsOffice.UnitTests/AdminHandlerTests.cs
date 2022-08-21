using System.Linq.Expressions;
using AutoMapper;
using DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;
using DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AdminHandlerTests
{
    private readonly IAdminRepository _fakeAdminRepository;
    private readonly IMapper _fakeMapper;

    public AdminHandlerTests()
    {
        _fakeMapper = A.Fake<IMapper>();
        _fakeAdminRepository = A.Fake<IAdminRepository>();
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminExists_ReturnsAdminWithSpecifiedId()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var admin = new Admin
        {
            AppUser = new AppUser {Id = adminId}
        };
        A.CallTo(() => _fakeAdminRepository.GetByIdAsync(adminId, A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(admin);

        var expectedResponse = new AdminResponse {Id = adminId};
        A.CallTo(() => _fakeMapper.Map<AdminResponse>(A<Admin>.Ignored)).Returns(expectedResponse);

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository, _fakeMapper);

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
        Admin? admin = null;
        A.CallTo(() => _fakeAdminRepository.GetByIdAsync(adminId, A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(admin);

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository, _fakeMapper);

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

        A.CallTo(() => _fakeAdminRepository.GetAll(A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(adminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, _fakeMapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var admin in adminsQueryable)
            result.Value.Should().ContainEquivalentOf(new AdminResponse {Id = admin.Id});
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreNoAdmins_ReturnsEmptyList()
    {
        // arrange
        var dummyAdminsQueryable = A.CollectionOfDummy<Admin>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll(A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(dummyAdminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository, _fakeMapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value.Should().BeEmpty();
    }
}