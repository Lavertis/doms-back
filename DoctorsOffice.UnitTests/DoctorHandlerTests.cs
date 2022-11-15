using DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;
using DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;
using DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;
using DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorsFiltered;
using DoctorsOffice.Application.Services.Users;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using DoctorsOffice.SendGrid.Service;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class DoctorHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IDoctorRepository _fakeDoctorRepository;
    private readonly IOptions<UrlSettings> _fakeUrlSettings;
    private readonly IUserService _fakeUserService;
    private readonly IWebHostEnvironment _fakeWebHostEnvironment;

    public DoctorHandlerTests()
    {
        _fakeDoctorRepository = A.Fake<IDoctorRepository>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _fakeWebHostEnvironment = A.Fake<IWebHostEnvironment>();
        _fakeUrlSettings = A.Fake<IOptions<UrlSettings>>();
    }

    [Fact]
    public async Task GetAllDoctorsHandler_ThereAreDoctors_ReturnsAllDoctors()
    {
        // arrange
        var doctors = new List<Doctor>();
        for (var i = 0; i < 3; i++)
        {
            doctors.Add(new Doctor
            {
                AppUser = new AppUser()
            });
        }

        var doctorResponses = doctors.Select(d => Mapper.Map<DoctorResponse>(d));
        A.CallTo(() => _fakeDoctorRepository.GetAll()).Returns(doctors.AsQueryable().BuildMock());

        var query = new GetDoctorsFilteredQuery();
        var handler = new GetDoctorsFilteredHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var doctorResponse in doctorResponses)
            result.Value!.Records.Should().ContainEquivalentOf(doctorResponse);
    }

    [Fact]
    public async Task GetAllDoctorsHandler_ThereAreNoDoctors_ReturnsEmptyList()
    {
        // arrange
        A.CallTo(() => _fakeDoctorRepository.GetAll())
            .Returns(A.CollectionOfDummy<Doctor>(0).AsQueryable().BuildMock());

        var query = new GetDoctorsFilteredQuery();
        var handler = new GetDoctorsFilteredHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDoctorsHandler_NoPaginationProvided_ReturnsAllDoctors()
    {
        // arrange
        var doctors = new List<Doctor>();
        for (var i = 0; i < 3; i++)
        {
            doctors.Add(new Doctor
            {
                AppUser = new AppUser()
            });
        }

        var expectedResponse = doctors.Select(d => Mapper.Map<DoctorResponse>(d));

        A.CallTo(() => _fakeDoctorRepository.GetAll())
            .Returns(doctors.AsQueryable().BuildMock());

        var query = new GetDoctorsFilteredQuery();
        var handler = new GetDoctorsFilteredHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAllDoctorsHandler_PaginationProvided_ReturnsRequestedPage()
    {
        // arrange
        var doctors = new List<Doctor>();
        for (var i = 0; i < 20; i++)
        {
            doctors.Add(new Doctor
            {
                AppUser = new AppUser()
            });
        }

        const int pageSize = 2;
        const int pageNumber = 3;

        var expectedResponse = doctors
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => Mapper.Map<DoctorResponse>(d));

        A.CallTo(() => _fakeDoctorRepository.GetAll())
            .Returns(doctors.AsQueryable().BuildMock());

        var query = new GetDoctorsFilteredQuery
        {
            PaginationFilter = new PaginationFilter {PageSize = pageSize, PageNumber = pageNumber}
        };
        var handler = new GetDoctorsFilteredHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value!.Records.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetDoctorByIdHandler_DoctorWithSpecifiedIdExists_ReturnsDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var doctor = new Doctor
        {
            Id = doctorId,
            AppUser = new AppUser {Id = doctorId}
        };
        var doctorsQueryable = new List<Doctor> {doctor}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeDoctorRepository.GetAll()).Returns(doctorsQueryable);

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = handler.Handle(query, default).Result;

        // assert
        result.Value.Should().BeEquivalentTo(Mapper.Map<DoctorResponse>(doctor));
    }

    [Fact]
    public async Task GetDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var doctorsQueryable = A.CollectionOfDummy<Doctor>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeDoctorRepository.GetAll()).Returns(doctorsQueryable);

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateDoctorHandler_ValidRequest_CreatesNewDoctor()
    {
        // arrange
        var request = new CreateDoctorRequest
        {
            Email = "mail@mail.com",
            PhoneNumber = "123456789",
        };
        var command = new CreateDoctorCommand(request);

        var appUser = new AppUser
        {
            UserName = command.Email,
            NormalizedUserName = command.Email.ToUpper(),
            Email = command.Email,
            NormalizedEmail = command.Email.ToUpper(),
            PhoneNumber = command.PhoneNumber,
            PasswordHash = Guid.NewGuid().ToString()
        };

        A.CallTo(() => _fakeUrlSettings.Value).Returns(new UrlSettings {FrontendDomain = "http://localhost:3000"});
        var handler = new CreateDoctorHandler(
            _fakeDoctorRepository,
            _fakeUserService,
            Mapper,
            A.Dummy<ISendGridService>(),
            A.Dummy<IOptions<SendGridTemplateSettings>>(),
            A.Dummy<IOptions<IdentitySettings>>(),
            _fakeWebHostEnvironment,
            _fakeAppUserManager,
            _fakeUrlSettings,
            A.Dummy<IOptions<QuickButtonSettings>>(),
            A.Dummy<IQuickButtonRepository>()
        );
        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored))
            .Returns(new HttpResult<AppUser>().WithValue(appUser));

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeDoctorRepository.CreateAsync(A<Doctor>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateDoctorByIdHandler_ValidRequest_UpdatesDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        var doctorToUpdate = new Doctor
        {
            Id = doctorId,
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "userName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789"
            }
        };

        var request = new UpdateDoctorRequest
        {
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            NewPassword = "newPassword1234#"
        };
        var command = new UpdateDoctorByIdCommand(request, doctorId);

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(doctorToUpdate);
        A.CallTo(() => _fakeAppUserManager.Users)
            .Returns(new List<AppUser> {doctorToUpdate.AppUser}.AsQueryable().BuildMock());

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeAppUserManager, Mapper);

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeAppUserManager.PasswordHasher.HashPassword(A<AppUser>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        var request = new UpdateDoctorRequest
        {
            Email = "newMail@mail.com",
            PhoneNumber = "123456789",
            NewPassword = "newPassword1234#"
        };
        var command = new UpdateDoctorByIdCommand(request, Guid.NewGuid());

        Doctor? doctor = null;
        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(doctor);

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(command, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData("Email", "newMail@mail.com")]
    [InlineData("PhoneNumber", "987654321")]
    [InlineData("NewPassword", "newPassword1234#")]
    public async Task UpdateDoctorByIdHandler_SingleFieldPresent_UpdatesSpecifiedField(
        string fieldName, string fieldValue)
    {
        // arrange
        var doctorId = Guid.NewGuid();
        var doctorToUpdate = new Doctor
        {
            Id = doctorId,
            AppUser = new AppUser
            {
                Id = doctorId,
                UserName = "userName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789"
            }
        };

        var request = new UpdateDoctorRequest();
        var command = new UpdateDoctorByIdCommand(request, doctorId);
        typeof(UpdateDoctorRequest).GetProperty(fieldName)!.SetValue(request, fieldValue);

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(doctorToUpdate);
        A.CallTo(() => _fakeAppUserManager.Users)
            .Returns(new List<AppUser> {doctorToUpdate.AppUser}.AsQueryable().BuildMock());

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeAppUserManager, Mapper);

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdExists_DeletesDoctor()
    {
        // arrange
        var doctorToDelete = new Doctor
        {
            AppUser = new AppUser {Id = Guid.NewGuid()}
        };
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithValue(doctorToDelete.AppUser));
        A.CallTo(() => _fakeAppUserManager.IsInRoleAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(true);
        var command = new DeleteDoctorByIdCommand(doctorToDelete.AppUser.Id);
        var handler = new DeleteDoctorByIdHandler(_fakeAppUserManager);

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeAppUserManager.DeleteByIdAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        var doctorId = Guid.NewGuid();
        A.CallTo(() => _fakeDoctorRepository.DeleteByIdAsync(A<Guid>.Ignored)).Returns(false);

        var command = new DeleteDoctorByIdCommand(doctorId);
        var handler = new DeleteDoctorByIdHandler(_fakeAppUserManager);

        // act
        var result = await handler.Handle(command, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}