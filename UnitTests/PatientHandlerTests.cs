using System.Linq.Expressions;
using DoctorsOfficeApi.CQRS.Commands.CreatePatient;
using DoctorsOfficeApi.CQRS.Commands.DeletePatientById;
using DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;
using DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PatientRepository;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class PatientHandlerTests
{
    private readonly IPatientRepository _fakePatientRepository;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly IUserService _fakeUserService;

    public PatientHandlerTests()
    {
        _fakePatientRepository = A.Fake<IPatientRepository>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientExists_ReturnsPatient()
    {
        // arrange
        var patient = new Patient
        {
            AppUser = new AppUser {Id = Guid.NewGuid()}
        };
        A.CallTo(() =>
                _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored, A<Expression<Func<Patient, object>>>.Ignored))
            .Returns(patient);

        var expectedResult = new PatientResponse(patient);

        var query = new GetPatientByIdQuery(patient.Id);
        var handler = new GetPatientByIdHandler(_fakePatientRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() =>
                _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored, A<Expression<Func<Patient, object>>>.Ignored))
            .Throws(new NotFoundException(""));

        var patientId = Guid.NewGuid();

        var query = new GetPatientByIdQuery(patientId);
        var handler = new GetPatientByIdHandler(_fakePatientRepository);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
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
            .Returns(newAppUser);


        var handler = new CreatePatientHandler(_fakePatientRepository, _fakeUserService);

        // act
        var result = await handler.Handle(command, default);

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
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = patientId,
                UserName = "oldUserName",
                NormalizedUserName = "oldUserName".ToUpper(),
                Email = "oldMail@mail.com",
                NormalizedEmail = "oldMail@mail.com".ToUpper(),
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);
        A.CallTo(() => _fakeUserManager.Users)
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
        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeUserService, _fakeUserManager);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakePatientRepository.UpdateByIdAsync(A<Guid>.Ignored, A<Patient>.Ignored))
            .MustHaveHappenedOnceExactly();
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
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = patientId,
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);
        A.CallTo(() => _fakeUserManager.Users)
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
        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeUserService, _fakeUserManager);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        switch (fieldName)
        {
            case "UserName":
                result.UserName.Should().Be(fieldValue);
                break;
            case "FirstName":
                result.FirstName.Should().Be(fieldValue);
                break;
            case "LastName":
                result.LastName.Should().Be(fieldValue);
                break;
            case "Email":
                result.Email.Should().Be(fieldValue);
                break;
            case "PhoneNumber":
                result.PhoneNumber.Should().Be(fieldValue);
                break;
            case "Address":
                result.Address.Should().Be(fieldValue);
                break;
            case "DateOfBirth":
                result.DateOfBirth.Should().Be(DateTime.Parse(fieldValue));
                break;
            case "NewPassword":
                A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored))
                    .MustHaveHappenedOnceExactly();
                break;
        }
    }

    [Fact]
    public async Task UpdatePatientByIdHandler_PatientWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakePatientRepository.GetByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var request = new UpdateAuthenticatedPatientRequest();
        var updatePatientCommand = new UpdatePatientByIdCommand(request, Guid.NewGuid());

        var handler = new UpdatePatientByIdHandler(_fakePatientRepository, _fakeUserService, _fakeUserManager);

        // act
        var action = async () => await handler.Handle(updatePatientCommand, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeletePatientByIdHandler_PatientExists_DeletesPatient()
    {
        // arrange
        var patientToDelete = new Patient
        {
            FirstName = "firstName",
            LastName = "lastName",
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
    public async Task DeletePatientByIdHandler_PatientWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        A.CallTo(() => _fakePatientRepository.DeleteByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var patientId = Guid.NewGuid();

        var command = new DeletePatientByIdCommand(patientId);
        var handler = new DeletePatientByIdHandler(_fakePatientRepository);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}