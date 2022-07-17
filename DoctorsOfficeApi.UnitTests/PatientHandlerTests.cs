using DoctorsOfficeApi.CQRS.Commands.CreatePatient;
using DoctorsOfficeApi.CQRS.Commands.DeletePatientById;
using DoctorsOfficeApi.CQRS.Commands.UpdatePatientById;
using DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.PatientService;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class PatientHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IPatientService _fakePatientService;
    private readonly IUserService _fakeUserService;

    public PatientHandlerTests()
    {
        _fakeUserService = A.Fake<IUserService>();
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _dbContext = new AppDbContext(dbContextOptions);

        _fakePatientService = A.Fake<IPatientService>();
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientExists_ReturnsPatient()
    {
        // arrange
        var patient = new Patient
        {
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };
        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored)).Returns(patient);

        var expectedResult = new PatientResponse(patient);

        var query = new GetPatientByIdQuery(patient.Id);
        var handler = new GetPatientByIdHandler(_fakePatientService);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetPatientByIdHandler_PatientDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var patientId = Guid.NewGuid();

        var query = new GetPatientByIdQuery(patientId);
        var handler = new GetPatientByIdHandler(_fakePatientService);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreatePatientHandler_ValidRequest_CreatesPatient()
    {
        // arrange
        var createPatientCommand = new CreatePatientCommand
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

        var newAppUser = new AppUser
        {
            UserName = createPatientCommand.UserName,
            NormalizedUserName = createPatientCommand.UserName.ToUpper(),
            Email = createPatientCommand.Email,
            NormalizedEmail = createPatientCommand.Email.ToUpper(),
            PhoneNumber = createPatientCommand.PhoneNumber
        };

        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored))
            .Returns(newAppUser);


        var handler = new CreatePatientHandler(_dbContext, _fakeUserService);

        // act
        var result = await handler.Handle(createPatientCommand, default);

        // assert
        var createdPatient = _dbContext.Patients.ToList().First(p => p.UserName == createPatientCommand.UserName);
        createdPatient.UserName.Should().Be(createPatientCommand.UserName);
        createdPatient.AppUser.NormalizedUserName.Should().Be(createPatientCommand.UserName.ToUpper());
        createdPatient.FirstName.Should().Be(createPatientCommand.FirstName);
        createdPatient.LastName.Should().Be(createPatientCommand.LastName);
        createdPatient.Email.Should().Be(createPatientCommand.Email);
        createdPatient.AppUser.NormalizedEmail.Should().Be(createPatientCommand.Email.ToUpper());
        createdPatient.PhoneNumber.Should().Be(createPatientCommand.PhoneNumber);
        createdPatient.Address.Should().Be(createPatientCommand.Address);
        createdPatient.DateOfBirth.Should().Be(createPatientCommand.DateOfBirth);
    }

    [Fact]
    public async Task UpdatePatientByIdHandler_ValidRequest_UpdatesUser()
    {
        // arrange
        const string oldPassword = "oldPassword12345#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(new AppUser(), oldPassword);
        var patientToUpdate = new Patient
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "oldUserName",
                NormalizedUserName = "oldUserName".ToUpper(),
                Email = "oldMail@mail.com",
                NormalizedEmail = "oldMail@mail.com".ToUpper(),
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        _dbContext.Patients.Add(patientToUpdate);
        await _dbContext.SaveChangesAsync();

        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);

        var updatePatientCommand = new UpdatePatientByIdCommand
        {
            Id = patientToUpdate.Id,
            UserName = "newUserName",
            FirstName = "newFirstName",
            LastName = "newLastName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            Address = "newAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(10.Days()),
            NewPassword = "newPassword1234#"
        };
        var handler = new UpdatePatientByIdHandler(_dbContext, _fakePatientService, _fakeUserService);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        var updatedPatient = _dbContext.Patients.First(p => p.Id == patientToUpdate.Id);
        updatedPatient.UserName.Should().Be(updatePatientCommand.UserName);
        updatedPatient.AppUser.NormalizedUserName.Should().Be(updatePatientCommand.UserName.ToUpper());
        updatedPatient.FirstName.Should().Be(updatePatientCommand.FirstName);
        updatedPatient.LastName.Should().Be(updatePatientCommand.LastName);
        updatedPatient.Email.Should().Be(updatePatientCommand.Email);
        updatedPatient.AppUser.NormalizedEmail.Should().Be(updatePatientCommand.Email.ToUpper());
        updatedPatient.PhoneNumber.Should().Be(updatePatientCommand.PhoneNumber);
        updatedPatient.Address.Should().Be(updatePatientCommand.Address);
        updatedPatient.DateOfBirth.Should().Be(updatePatientCommand.DateOfBirth);
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
    public async Task UpdatePatientByIdHandler_SingleFieldPresent_UpdatesSpecifiedField(string fieldName, string fieldValue)
    {
        // arrange
        const string oldPassword = "oldPassword12345#";
        var oldPasswordHash = new PasswordHasher<AppUser>().HashPassword(new AppUser(), oldPassword);
        var patientToUpdate = new Patient
        {
            FirstName = "oldFirstName",
            LastName = "oldLastName",
            Address = "oldAddress",
            DateOfBirth = DateTime.UtcNow.Subtract(5.Days()),
            AppUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "oldUserName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789",
                PasswordHash = oldPasswordHash
            }
        };

        _dbContext.Patients.Add(patientToUpdate);
        await _dbContext.SaveChangesAsync();

        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Returns(patientToUpdate);

        var updatePatientCommand = new UpdatePatientByIdCommand
        {
            Id = patientToUpdate.Id,
        };
        if (fieldName == "DateOfBirth")
            updatePatientCommand.DateOfBirth = DateTime.Parse(fieldValue);
        else
            typeof(UpdatePatientByIdCommand).GetProperty(fieldName)!.SetValue(updatePatientCommand, fieldValue);

        var handler = new UpdatePatientByIdHandler(_dbContext, _fakePatientService, _fakeUserService);

        // act
        var result = await handler.Handle(updatePatientCommand, default);

        // assert
        var updatedPatient = _dbContext.Patients.First(p => p.Id == patientToUpdate.Id);
        switch (fieldName)
        {
            case "UserName":
                updatedPatient.UserName.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedUserName.Should().Be(fieldValue.ToUpper());
                break;
            case "FirstName":
                updatedPatient.FirstName.Should().Be(fieldValue);
                break;
            case "LastName":
                updatedPatient.LastName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedPatient.Email.Should().Be(fieldValue);
                updatedPatient.AppUser.NormalizedEmail.Should().Be(fieldValue.ToUpper());
                break;
            case "PhoneNumber":
                updatedPatient.PhoneNumber.Should().Be(fieldValue);
                break;
            case "Address":
                updatedPatient.Address.Should().Be(fieldValue);
                break;
            case "DateOfBirth":
                updatedPatient.DateOfBirth.Should().Be(DateTime.Parse(fieldValue));
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
        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var updatePatientCommand = new UpdatePatientByIdCommand
        {
            Id = Guid.NewGuid(),
        };

        var handler = new UpdatePatientByIdHandler(_dbContext, _fakePatientService, _fakeUserService);

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
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };

        _dbContext.Patients.Add(patientToDelete);
        await _dbContext.SaveChangesAsync();


        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Returns(patientToDelete);

        var command = new DeletePatientByIdCommand(patientToDelete.Id);
        var handler = new DeletePatientByIdHandler(_dbContext, _fakePatientService);

        // act
        await handler.Handle(command, default);

        // assert
        _dbContext.Patients.Any(p => p.Id == patientToDelete.Id).Should().BeFalse();
    }

    [Fact]
    public async Task DeletePatientByIdHandler_PatientWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        A.CallTo(() => _fakePatientService.GetPatientByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var patientId = Guid.NewGuid();

        var command = new DeletePatientByIdCommand(patientId);
        var handler = new DeletePatientByIdHandler(_dbContext, _fakePatientService);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}