using DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;
using DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;
using DoctorsOffice.Application.CQRS.Commands.Patients.UpdatePatientById;
using DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientById;
using DoctorsOffice.Application.Services.Users;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using DoctorsOffice.SendGrid.Service;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class PatientHandlerTests : UnitTest
{
    private readonly AppUserManager _fakeAppUserManager;
    private readonly IPatientRepository _fakePatientRepository;
    private readonly IOptions<UrlSettings> _fakeUrlSettings;
    private readonly IUserService _fakeUserService;

    public PatientHandlerTests()
    {
        _fakePatientRepository = A.Fake<IPatientRepository>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _fakeUrlSettings = A.Fake<IOptions<UrlSettings>>();
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientExists_ReturnsPatient()
    {
        // arrange
        var patient = new Patient
        {
            AppUser = new AppUser {Id = Guid.NewGuid()}
        };
        var patientsQueryable = new List<Patient> {patient}.AsQueryable().BuildMock();
        A.CallTo(() => _fakePatientRepository.GetAll()).Returns(patientsQueryable);

        var expectedResult = Mapper.Map<PatientResponse>(patient);

        var query = new GetPatientByIdQuery(patient.Id);
        var handler = new GetPatientByIdHandler(_fakePatientRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientDoesntExist_ReturnsNotFound404StatusCodes()
    {
        // arrange
        var patientsQueryable = A.CollectionOfDummy<Patient>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakePatientRepository.GetAll()).Returns(patientsQueryable);

        var patientId = Guid.NewGuid();

        var query = new GetPatientByIdQuery(patientId);
        var handler = new GetPatientByIdHandler(_fakePatientRepository, Mapper);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreatePatientHandler_ValidRequest_CreatesPatient()
    {
        // arrange
        var request = new CreatePatientRequest
        {
            UserName = "testUserName",
            FirstName = "John",
            LastName = "Doe",
            Email = "mail@mail.com",
            PhoneNumber = "123456789",
            Address = "TestAddress",
            DateOfBirth = DateTime.UtcNow,
            Password = "TestPassword12345#"
        };
        var command = new CreatePatientCommand(request);

        var newAppUser = new AppUser
        {
            UserName = command.UserName,
            NormalizedUserName = command.UserName.ToUpper(),
            Email = command.Email,
            NormalizedEmail = command.Email.ToUpper(),
            PhoneNumber = command.PhoneNumber
        };

        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored))
            .Returns(new HttpResult<AppUser>().WithValue(newAppUser));
        A.CallTo(() => _fakeUrlSettings.Value).Returns(new UrlSettings {FrontendDomain = "http://localhost:3000"});

        var handler = new CreatePatientHandler(
            _fakePatientRepository,
            _fakeUserService,
            _fakeAppUserManager,
            _fakeUrlSettings,
            A.Dummy<ISendGridService>(),
            A.Dummy<IOptions<SendGridTemplateSettings>>(),
            A.Dummy<IOptions<IdentitySettings>>(),
            A.Dummy<IWebHostEnvironment>()
        );

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePatientRepository.CreateAsync(A<Patient>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdatePatientByIdHandler_ValidRequest_UpdatesUser()
    {
        // arrange
        var patientId = Guid.NewGuid();
        const string oldPassword = "oldPassword12345#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(new AppUser(), oldPassword);
        var patientToUpdate = new Patient
        {
            Id = patientId,
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = patientId,
                UserName = "oldUserName",
                NormalizedUserName = "oldUserName".ToUpper(),
                Email = "oldMail@mail.com",
                NormalizedEmail = "oldMail@mail.com".ToUpper(),
                FirstName = "oldFirstName",
                LastName = "oldLastName",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);
        A.CallTo(() => _fakeAppUserManager.Users)
            .Returns(new List<AppUser> {patientToUpdate.AppUser}.AsQueryable().BuildMock());

        var request = new UpdateAuthenticatedPatientRequest
        {
            UserName = "newUserName",
            FirstName = "newFirstName",
            LastName = "newLastName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            Address = "newAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            NewPassword = "newPassword1234#"
        };
        var updatePatientCommand = new UpdatePatientByIdCommand(request, patientId);
        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeAppUserManager, Mapper);

        // act
        await handler.Handle(updatePatientCommand, default);

        // assert
        A.CallTo(() => _fakeAppUserManager.PasswordHasher.HashPassword(A<AppUser>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakePatientRepository.UpdateAsync(A<Patient>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("UserName", "newUserName")]
    [InlineData("FirstName", "newFirstName")]
    [InlineData("LastName", "newLastName")]
    [InlineData("Email", "newMail@mail.com")]
    [InlineData("PhoneNumber", "987654321")]
    [InlineData("Address", "newAddress")]
    [InlineData("DateOfBirth", "2020-01-01")]
    [InlineData("NewPassword", "newPassword1234#")]
    public async Task UpdatePatientByIdHandler_SingleFieldPresent_UpdatesSpecifiedField(string fieldName,
        string fieldValue)
    {
        // arrange
        const string oldPassword = "oldPassword12345#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(new AppUser(), oldPassword);
        var patientId = Guid.NewGuid();
        var patientToUpdate = new Patient
        {
            Id = patientId,
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = patientId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                FirstName = "oldFirstName",
                LastName = "oldLastName",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);
        A.CallTo(() => _fakeAppUserManager.Users)
            .Returns(new List<AppUser> {patientToUpdate.AppUser}.AsQueryable().BuildMock());

        var request = new UpdateAuthenticatedPatientRequest();
        if (fieldName == "DateOfBirth")
        {
            request.DateOfBirth = DateTime.Parse(fieldValue);
        }
        else
        {
            typeof(UpdateAuthenticatedPatientRequest)
                .GetProperty(fieldName)!
                .SetValue(request, fieldValue);
        }

        var updatePatientCommand = new UpdatePatientByIdCommand(request, patientId);
        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        switch (fieldName)
        {
            case "UserName":
                result.Value!.UserName.Should().Be(fieldValue);
                break;
            case "FirstName":
                result.Value!.FirstName.Should().Be(fieldValue);
                break;
            case "LastName":
                result.Value!.LastName.Should().Be(fieldValue);
                break;
            case "NationalId":
                result.Value!.NationalId.Should().Be(fieldValue);
                break;
            case "Email":
                result.Value!.Email.Should().Be(fieldValue);
                break;
            case "PhoneNumber":
                result.Value!.PhoneNumber.Should().Be(fieldValue);
                break;
            case "Address":
                result.Value!.Address.Should().Be(fieldValue);
                break;
            case "DateOfBirth":
                result.Value!.DateOfBirth.Should().Be(DateTime.Parse(fieldValue));
                break;
            case "NewPassword":
                A.CallTo(() => _fakeAppUserManager.PasswordHasher.HashPassword(A<AppUser>.Ignored, A<string>.Ignored))
                    .MustHaveHappenedOnceExactly();
                break;
        }
    }

    [Fact]
    public async Task UpdatePatientByIdHandler_PatientWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        Patient? patient = null;
        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored)).Returns(patient);

        var request = new UpdateAuthenticatedPatientRequest();
        var updatePatientCommand = new UpdatePatientByIdCommand(request, Guid.NewGuid());

        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeAppUserManager, Mapper);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeletePatientByIdHandler_PatientExists_DeletesPatient()
    {
        // arrange
        var patientToDelete = new Patient
        {
            Address = "address",
            AppUser = new AppUser {Id = Guid.NewGuid()}
        };

        var command = new DeletePatientByIdCommand(patientToDelete.Id);
        var handler = new DeletePatientByIdHandler(_fakePatientRepository);

        // act
        await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakePatientRepository.DeleteByIdAsync(A<Guid>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeletePatientByIdHandler_PatientWithSpecifiedIdDoesntExist_ReturnsNotFound404StatusCode()
    {
        A.CallTo(() => _fakePatientRepository.DeleteByIdAsync(A<Guid>.Ignored)).Returns(false);

        var patientId = Guid.NewGuid();

        var command = new DeletePatientByIdCommand(patientId);
        var handler = new DeletePatientByIdHandler(_fakePatientRepository);

        // act
        var result = await handler.Handle(command, default);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}