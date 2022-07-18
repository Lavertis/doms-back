﻿using System.Linq.Expressions;
using DoctorsOfficeApi.CQRS.Commands.CreateDoctor;
using DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;
using DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;
using DoctorsOfficeApi.CQRS.Queries.GetAllDoctors;
using DoctorsOfficeApi.CQRS.Queries.GetDoctorById;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Services.UserService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOfficeApi.UnitTests;

public class DoctorHandlerTests
{
    private readonly IDoctorRepository _fakeDoctorRepository;
    private readonly IUserService _fakeUserService;
    private readonly UserManager<AppUser> _fakeUserManager;

    public DoctorHandlerTests()
    {
        _fakeDoctorRepository = A.Fake<IDoctorRepository>();
        _fakeUserService = A.Fake<IUserService>();
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
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

        A.CallTo(() => _fakeDoctorRepository.GetAll(A<Expression<Func<Doctor, object>>>.Ignored))
            .Returns(doctors.AsQueryable().BuildMock());

        var query = new GetAllDoctorsQuery();
        var handler = new GetAllDoctorsHandler(_fakeDoctorRepository);

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
        A.CallTo(() => _fakeDoctorRepository.GetAll(A<Expression<Func<Doctor, object>>>.Ignored))
            .Returns(A.CollectionOfDummy<Doctor>(0).AsQueryable().BuildMock());

        var query = new GetAllDoctorsQuery();
        var handler = new GetAllDoctorsHandler(_fakeDoctorRepository);

        // act
        var result = await handler.Handle(query, default);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDoctorByIdHandler_DoctorWithSpecifiedIdExists_ReturnsDoctor()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        var doctor = new Doctor
        {
            AppUser = new AppUser { Id = doctorId }
        };

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(doctorId, A<Expression<Func<Doctor, object>>>.Ignored))
            .Returns(doctor);

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorRepository);

        // act
        var result = handler.Handle(query, default).Result;

        // assert
        result.Should().BeEquivalentTo(new DoctorResponse(doctor));
    }

    [Fact]
    public async Task GetDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        var doctor = new Doctor
        {
            AppUser = new AppUser { Id = doctorId }
        };

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(doctorId, A<Expression<Func<Doctor, object>>>.Ignored))
            .Throws(new NotFoundException(""));

        var query = new GetDoctorByIdQuery(doctorId);
        var handler = new GetDoctorByIdHandler(_fakeDoctorRepository);

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

        var handler = new CreateDoctorHandler(_fakeDoctorRepository, _fakeUserService);
        A.CallTo(() => _fakeUserService.CreateUserAsync(A<CreateUserRequest>.Ignored)).Returns(appUser);

        // act
        var result = await handler.Handle(command, default);

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

        var command = new UpdateDoctorByIdCommand
        {
            Id = doctorId,
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "987654321",
            Password = "newPassword1234#"
        };

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(doctorToUpdate);
        A.CallTo(() => _fakeUserManager.Users).Returns(new List<AppUser> { doctorToUpdate.AppUser }.AsQueryable().BuildMock());

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeUserService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeUserService.SetUserPassword(A<AppUser>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateDoctorByIdHandler_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        var command = new UpdateDoctorByIdCommand
        {
            Id = Guid.NewGuid(),
            UserName = "newUserName",
            Email = "newMail@mail.com",
            PhoneNumber = "123456789",
            Password = "newPassword1234#"
        };

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeUserService, _fakeUserManager);

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

        var command = new UpdateDoctorByIdCommand
        {
            Id = doctorId,
        };
        typeof(UpdateDoctorByIdCommand).GetProperty(fieldName)!.SetValue(command, fieldValue);

        A.CallTo(() => _fakeDoctorRepository.GetByIdAsync(A<Guid>.Ignored))
            .Returns(doctorToUpdate);
        A.CallTo(() => _fakeUserManager.Users).Returns(new List<AppUser> { doctorToUpdate.AppUser }.AsQueryable().BuildMock());

        var handler = new UpdateDoctorByIdHandler(_fakeDoctorRepository, _fakeUserService, _fakeUserManager);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdExists_DeletesDoctor()
    {
        // arrange
        var doctorToDelete = new Doctor
        {
            AppUser = new AppUser { Id = Guid.NewGuid() }
        };

        var command = new DeleteDoctorByIdCommand(doctorToDelete.AppUser.Id);
        var handler = new DeleteDoctorByIdHandler(_fakeDoctorRepository);

        // act
        var result = await handler.Handle(command, default);

        // assert
        A.CallTo(() => _fakeDoctorRepository.DeleteByIdAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDoctorById_DoctorWithSpecifiedIdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        var doctorId = Guid.NewGuid();

        A.CallTo(() => _fakeDoctorRepository.DeleteByIdAsync(A<Guid>.Ignored))
            .Throws(new NotFoundException(""));

        var command = new DeleteDoctorByIdCommand(doctorId);
        var handler = new DeleteDoctorByIdHandler(_fakeDoctorRepository);

        // act
        var action = async () => await handler.Handle(command, default);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }
}