using DoctorsOffice.Application.Services.Users;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class UserServiceTests : UnitTest
{
    private readonly AppRoleManager _fakeAppRoleManager;
    private readonly AppUserManager _fakeAppUserManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _fakeAppRoleManager = A.Fake<AppRoleManager>();
        _fakeAppUserManager = A.Fake<AppUserManager>();
        _userService = new UserService(_fakeAppUserManager, _fakeAppRoleManager, Mapper);
    }

    [Fact]
    public async void CreateUserAsync_ValidData_ReturnsSuccessfulResult()
    {
        // arrange
        var fakeIdentityResult = IdentityResult.Success;
        A.CallTo(() => _fakeAppRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(true);
        A.CallTo(() => _fakeAppUserManager.CreateAsync(A<AppUser>.Ignored, A<string>.Ignored))
            .Returns(fakeIdentityResult);
        A.CallTo(() =>
            _fakeAppUserManager.AddToRoleAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(fakeIdentityResult
        );
        A.CallTo(() => _fakeAppUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var createUserRequest = new CreateUserRequest
        {
            UserName = "testUserName",
            Email = "testEmail@mail.com",
            PhoneNumber = "123456789",
            Password = "testPassword1235%",
            RoleName = "testRole"
        };

        // act
        var result = await _userService.CreateUserAsync(createUserRequest);

        // assert
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async void CreateUserAsync_RoleNameDoesntExist_ReturnsBadRequest400StatusCode()
    {
        // arrange
        A.CallTo(() => _fakeAppRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(false);
        A.CallTo(() => _fakeAppUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var createUserRequest = new CreateUserRequest
        {
            UserName = "testUserName",
            Email = "testEmail@mail.com",
            PhoneNumber = "123456789",
            Password = "testPassword1235%",
            RoleName = "notExistingRole"
        };

        // act
        var result = await _userService.CreateUserAsync(createUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("Email", "")]
    [InlineData("Email", "invalidEmail")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Password", "")]
    [InlineData("RoleName", "")]
    public async void CreateUserAsync_InvalidProperty_ReturnsBadRequest400StatusCode(string propertyName,
        string propertyValue)
    {
        // arrange
        A.CallTo(() => _fakeAppRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(true);
        A.CallTo(() => _fakeAppUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var createUserRequest = new CreateUserRequest
        {
            UserName = "testUserName",
            Email = "testEmail@mail.com",
            PhoneNumber = "123456789",
            Password = "testPassword1235%",
            RoleName = "notExistingRole"
        };
        typeof(CreateUserRequest).GetProperty(propertyName)!.SetValue(createUserRequest, propertyValue);

        // act
        var result = await _userService.CreateUserAsync(createUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void UpdateUserByIdAsync_RequestIsValid_UpdatesUser()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        const string testUserName = "testUserName";
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = testUserName,
            NormalizedUserName = testUserName.ToUpper(),
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithValue(appUser));
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);

        var updateUserRequest = new UpdateUserRequest
        {
            UserName = "newUerName",
            Email = "mail@mail.com",
            PhoneNumber = "newPhoneNumber",
            NewPassword = "newPassword",
            ConfirmPassword = "newPassword"
        };

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        result.Value!.UserName.Should().Be(updateUserRequest.UserName);
        result.Value.NormalizedUserName.Should().Be(updateUserRequest.UserName.ToUpper());
        result.Value.Email.Should().Be(updateUserRequest.Email);
        result.Value.NormalizedEmail.Should().Be(updateUserRequest.Email.ToUpper());
        result.Value.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);
        result.Value.PasswordHash.Should().NotBe(oldPasswordHash);
        result.Value.EmailConfirmed.Should().BeTrue();
        result.Value.PhoneNumberConfirmed.Should().BeTrue();
        result.Value.PasswordHash.Should().NotBe(oldPasswordHash);
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("UserName", "newUerName")]
    [InlineData("Email", "mail@mail.com")]
    [InlineData("PhoneNumber", "newPhoneNumber")]
    [InlineData("NewPassword", "newPassword")]
    public async void UpdateUserByIdAsync_SingleFieldInRequest_UpdatesRequestedField(string fieldName,
        string fieldValue)
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        const string testUserName = "testUserName";
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = testUserName,
            NormalizedUserName = testUserName.ToUpper(),
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<Guid>.Ignored))
            .Returns(new CommonResult<AppUser>().WithValue(appUser));
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);

        var updateUserRequest = new UpdateUserRequest();
        typeof(UpdateUserRequest).GetProperty(fieldName)!.SetValue(updateUserRequest, fieldValue);
        if (updateUserRequest.NewPassword is not null)
        {
            updateUserRequest.ConfirmPassword = updateUserRequest.NewPassword;
        }

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        if (updateUserRequest.UserName is not null) result.Value!.UserName.Should().Be(updateUserRequest.UserName);
        if (updateUserRequest.UserName is not null)
            result.Value!.NormalizedUserName.Should().Be(updateUserRequest.UserName.ToUpper());
        if (updateUserRequest.Email is not null) result.Value!.Email.Should().Be(updateUserRequest.Email);
        if (updateUserRequest.Email is not null)
            result.Value!.NormalizedEmail.Should().Be(updateUserRequest.Email.ToUpper());
        if (updateUserRequest.PhoneNumber is not null)
            result.Value!.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);
        if (updateUserRequest.Email is not null) result.Value!.EmailConfirmed.Should().Be(true);
        if (updateUserRequest.PhoneNumber is not null) result.Value!.PhoneNumberConfirmed.Should().Be(true);
        if (updateUserRequest.NewPassword is not null) result.Value!.PasswordHash.Should().NotBe(oldPasswordHash);
        A.CallTo(() => _fakeAppUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("UserName", "a")]
    [InlineData("Email", "mail_mail.com")]
    public async void UpdateUserByIdAsync_SingleInvalidField_ReturnsBadRequest400StatusCode(string fieldName,
        string fieldValue)
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        const string testUserName = "testUserName";
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = testUserName,
            NormalizedUserName = testUserName.ToUpper(),
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);

        var updateUserRequest = new UpdateUserRequest
        {
            UserName = "newUerName",
            Email = "newEmail",
            PhoneNumber = "newPhoneNumber",
            NewPassword = "newPassword",
            ConfirmPassword = "newPassword"
        };

        typeof(UpdateUserRequest).GetProperty(fieldName)!.SetValue(updateUserRequest, fieldValue);

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void UpdateUserByIdAsync_IdDoesntExist_ReturnsNotFound404StatusCode()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeAppUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var id = Guid.NewGuid();
        var updateUserRequest = A.Dummy<UpdateUserRequest>();

        // act
        var result = await _userService.UpdateUserByIdAsync(id, updateUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async void UpdateUserByIdAsync_RequestedUserNameIsAlreadyTaken_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser {Id = Guid.NewGuid(), UserName = "testUserName", PasswordHash = oldPasswordHash};
        var conflictingUser = new AppUser {Id = Guid.NewGuid(), UserName = "conflictingUserName"};
        var usersQueryable = new List<AppUser> {appUser, conflictingUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);

        var updateUserRequest = new UpdateUserRequest {UserName = "conflictingUserName"};

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async void UpdateUserByIdAsync_NewPasswordAndConfirmPasswordDontMatch_ReturnsBadRequest400StatusCode()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        const string testUserName = "testUserName";
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(), UserName = testUserName, NormalizedUserName = testUserName.ToUpper(),
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeAppUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeAppUserManager.Users).Returns(usersQueryable);

        var updateUserRequest = new UpdateUserRequest
        {
            NewPassword = "StrongNewPassword12#",
            ConfirmPassword = "NotMatchingNewPassword12#"
        };

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}