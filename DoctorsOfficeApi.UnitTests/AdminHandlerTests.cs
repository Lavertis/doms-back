using System.Linq.Expressions;
using DoctorsOfficeApi.CQRS.Queries.GetAdminById;
using DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.AdminRepository;
using FakeItEasy;
using FluentAssertions;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class AdminHandlerTests
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
        var admin = new Admin
        {
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };
        A.CallTo(() => _fakeAdminRepository.GetByIdAsync(adminId, A<Expression<Func<Admin, object>>>.Ignored)).Returns(admin);

        var expectedResponse = new AdminResponse(admin);

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAdminByIdHandler_AdminDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var adminId = Guid.NewGuid();
        A.CallTo(() => _fakeAdminRepository.GetByIdAsync(adminId, A<Expression<Func<Admin, object>>>.Ignored))
            .Throws(new NotFoundException(""));

        var query = new GetAdminByIdQuery(adminId);
        var handler = new GetAdminByIdHandler(_fakeAdminRepository);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreAdmins_ReturnsAllAdmins()
    {
        // arrange
        var adminsQueryable = new List<Admin>
        {
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } },
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } },
            new() { AppUser = new AppUser { Id = Guid.NewGuid() } }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAdminRepository.GetAll(A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(adminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var admin in adminsQueryable)
            result.Should().ContainEquivalentOf(new AdminResponse(admin));
    }

    [Fact]
    public async Task GetAllAdminsHandler_ThereAreNoAdmins_ReturnsEmptyList()
    {
        // arrange
        var dummyAdminsQueryable = A.CollectionOfDummy<Admin>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeAdminRepository.GetAll(A<Expression<Func<Admin, object>>>.Ignored))
            .Returns(dummyAdminsQueryable);

        var query = new GetAllAdminsQuery();
        var handler = new GetAllAdminsHandler(_fakeAdminRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }
}