using DoctorsOfficeApi.CQRS.Commands.CreateDoctor;
using DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;
using DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;
using DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;
using DoctorsOfficeApi.CQRS.Queries.GetDoctorById;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.DoctorService;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class DoctorHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IDoctorService _fakeDoctorService;
    private readonly IUserService _fakeUserService;

    public DoctorHandlerTests()
    {
        var inMemoryDbName = "InMemoryDb_" + Guid.NewGuid();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(inMemoryDbName)
            .Options;
        _dbContext = new AppDbContext(dbContextOptions);

        _fakeDoctorService = A.Fake<IDoctorService>();
        _fakeUserService = A.Fake<IUserService>();
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

        var doctorResponses = doctors.Select(d => new DoctorResponse(d));

        _dbContext.Doctors.AddRange(doctors);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllDoctorsQuery();
        var handler = new GetAllDoctorsHandler(_dbContext);

        // act
        var result = await handler.Handle(query, default);

        // assert
        foreach (var doctorResponse in doctorResponses)
            result.Should().ContainEquivalentOf(doctorResponse);
    }

    [Fact]
    public async Task GetAllDoctorsHandler_ThereAreNoDoctors_ReturnsEmptyList()
    {
        // arrange
        _dbContext.RemoveRange(_dbContext.Doctors);

        var query = new GetAllDoctorsQuery();
        var handler = new GetAllDoctorsHandler(_dbContext);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDoctorByIdHandler_DoctorWithSpecifiedIdExists_ReturnsDoctor()
    {
        // arrange
        const string doctorId = "1";

        var doctor = new Doctor
        {
            AppUser = new AppUser { Id = doctorId }
        };

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(doctorId)).Returns(doctor);

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorService);

        // act
        var result = handler.Handle(query, default).Result;

        // assert
        result.Should().BeEquivalentTo(new DoctorResponse(doctor));
    }

    [Fact]
    public async Task GetDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string doctorId = "1";

        var doctor = new Doctor
        {
            AppUser = new AppUser { Id = doctorId }
        };

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(doctorId)).Throws(new NotFoundException(""));

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorService);

        // act
        var action = async () => await handler.Handle(query, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateDoctorHandler_ValidRequest_CreatesNewDoctor()
    {
        // arrange

        var command = new CreateDoctorCommand
        {
            UserName = "userName",
            Email = "mail@mail.com",
            PhoneNumber = "123456789",
            Password = "Password1234#"
        };

        var appUser = new AppUser
        {
            UserName = command.UserName,
            NormalizedUserName = command.UserName.ToUpper(),
            Email = command.Email,
            NormalizedEmail = command.Email.ToUpper(),
            PhoneNumber = command.PhoneNumber,
            PasswordHash = command.Password
        };

        var handler = new CreateDoctorHandler(_dbContext, _fakeUserService);
        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored)).Returns(appUser);

        // act
        var result = await handler.Handle(command, default);

        // assert
        var createdDoctor = _dbContext.Doctors.FirstOrDefault(d => d.AppUser.UserName == command.UserName);

        createdDoctor.Should().NotBeNull();
        createdDoctor!.AppUser.NormalizedUserName.Should().Be(command.UserName.ToUpper());
        createdDoctor.AppUser.Email.Should().Be(command.Email);
        createdDoctor.AppUser.NormalizedEmail.Should().Be(command.Email.ToUpper());
        createdDoctor.AppUser.PhoneNumber.Should().Be(command.PhoneNumber);
        createdDoctor.AppUser.PasswordHash.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateDoctorByIdHandler_ValidRequest_UpdatesDoctor()
    {
        // arrange
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = "1",
                UserName = "userName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789"
            }
        };

        _dbContext.Doctors.Add(doctorToUpdate);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateDoctorByIdCommand
        {
            Id = "1",
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            Password = "newPassword1234#"
        };

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Returns(doctorToUpdate);

        var handler = new UpdateDoctorByIdHandler(_dbContext, _fakeDoctorService, _fakeUserService);

        // act
        var result = await handler.Handle(command, default);

        // assert
        var updatedDoctor = _dbContext.Doctors.FirstOrDefault(d => d.AppUser.Id == command.Id);
        updatedDoctor.Should().NotBeNull();
        updatedDoctor!.AppUser.UserName.Should().Be(command.UserName);
        updatedDoctor.AppUser.Email.Should().Be(command.Email);
        updatedDoctor.AppUser.PhoneNumber.Should().Be(command.PhoneNumber);
        A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        var command = new UpdateDoctorByIdCommand
        {
            Id = "1",
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "123456789",
            Password = "newPassword1234#"
        };

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));

        var handler = new UpdateDoctorByIdHandler(_dbContext, _fakeDoctorService, _fakeUserService);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("UserName", "newUserName")]
    [InlineData("Email", "newMail@mail.com")]
    [InlineData("PhoneNumber", "987654321")]
    [InlineData("Password", "newPassword1234#")]
    public async Task UpdateDoctorByIdHandler_SingleFieldPresent_UpdatesSpecifiedField(
        string fieldName, string fieldValue)
    {
        // arrange
        var doctorToUpdate = new Doctor
        {
            AppUser = new AppUser
            {
                Id = "1",
                UserName = "userName",
                Email = "oldMail@mail.com",
                PhoneNumber = "123456789"
            }
        };

        _dbContext.Doctors.Add(doctorToUpdate);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateDoctorByIdCommand
        {
            Id = "1"
        };
        typeof(UpdateDoctorByIdCommand).GetProperty(fieldName)!.SetValue(command, fieldValue);

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Returns(doctorToUpdate);

        var handler = new UpdateDoctorByIdHandler(_dbContext, _fakeDoctorService, _fakeUserService);

        // act
        var result = await handler.Handle(command, default);

        // assert
        var updatedDoctor = _dbContext.Doctors.FirstOrDefault(d => d.AppUser.Id == command.Id);
        updatedDoctor.Should().NotBeNull();

        switch (fieldName)
        {
            case "UserName":
                updatedDoctor!.AppUser.UserName.Should().Be(fieldValue);
                break;
            case "Email":
                updatedDoctor!.AppUser.Email.Should().Be(fieldValue);
                break;
            case "PhoneNumber":
                updatedDoctor!.AppUser.PhoneNumber.Should().Be(fieldValue);
                break;
            case "Password":
                A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
                break;
        }
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdExists_DeletesDoctor()
    {
        // arrange
        var doctorToDelete = new Doctor
        {
            AppUser = new AppUser { Id = "1" }
        };
        _dbContext.Doctors.Add(doctorToDelete);

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored)).Returns(doctorToDelete);

        var command = new DeleteDoctorByIdCommand(doctorToDelete.AppUser.Id);
        var handler = new DeleteDoctorByIdHandler(_dbContext, _fakeDoctorService);

        // act
        var result = await handler.Handle(command, default);

        // assert
        var deletedDoctor = _dbContext.Doctors.FirstOrDefault(d => d.AppUser.Id == command.Id);
        deletedDoctor.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        const string doctorId = "1";

        A.CallTo(() => _fakeDoctorService.GetDoctorByIdAsync(A<string>.Ignored))
            .Throws(new NotFoundException(""));

        var command = new DeleteDoctorByIdCommand(doctorId);
        var handler = new DeleteDoctorByIdHandler(_dbContext, _fakeDoctorService);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}