﻿using System.Security.Claims;
using AutoMapper;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Exceptions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.FakeItEasy;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class UserServiceTests
{
    private readonly IMapper _fakeMapper;
    private readonly RoleManager<AppRole> _fakeRoleManager;
    private readonly UserManager<AppUser> _fakeUserManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _fakeRoleManager = A.Fake<RoleManager<AppRole>>();
        _fakeUserManager = A.Fake<UserManager<AppUser>>();
        _fakeMapper = A.Fake<IMapper>();
        _userService = new UserService(_fakeUserManager, _fakeRoleManager, _fakeMapper);
    }

    [Fact]
    public async void GetUserByIdAsync_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var action = async () => await _userService.GetUserByIdAsync(Guid.NewGuid());

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async void GetUserByIdAsync_IdExists_ReturnsUser()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var result = await _userService.GetUserByIdAsync(Guid.NewGuid());

        // assert
        result.Should().Be(appUser);
    }

    [Fact]
    public async void GetUserByUserName_UserNameExists_ReturnsUser()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();

        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var result = await _userService.GetUserByUserNameAsync("test");

        // assert
        result.Should().Be(appUser);
    }

    [Fact]
    public async void GetUserByUserName_UserNameDoesntExits_ThrowsNotFoundException()
    {
        // arrange
        AppUser appUser = null!;

        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var action = async () => await _userService.GetUserByUserNameAsync("test");

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async void CreateUserAsync_ValidData_DoesNotThrowAnyException()
    {
        // arrange
        var fakeIdentityResult = IdentityResult.Success;
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(true);
        A.CallTo(() => _fakeUserManager.CreateAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(fakeIdentityResult);
        A.CallTo(() =>
            _fakeUserManager.AddToRoleAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(fakeIdentityResult
        );
        A.CallTo(() => _fakeUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var createUserRequest = new CreateUserRequest
        {
            UserName = "testUserName",
            Email = "testEmail@mail.com",
            PhoneNumber = "123456789",
            Password = "testPassword1235%",
            RoleName = "testRole"
        };
        A.CallTo(() => _fakeMapper.Map<AppUser>(A<CreateUserRequest>.Ignored)).Returns(
            new AppUser
            {
                UserName = createUserRequest.UserName,
                Email = createUserRequest.Email,
                PhoneNumber = createUserRequest.PhoneNumber
            }
        );

        // act
        var action = async () => await _userService.CreateUserAsync(createUserRequest);

        // assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async void CreateUserAsync_RoleNameDoesntExist_ThrowsBadRequestException()
    {
        // arrange
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(false);
        A.CallTo(() => _fakeUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var createUserRequest = new CreateUserRequest
        {
            UserName = "testUserName",
            Email = "testEmail@mail.com",
            PhoneNumber = "123456789",
            Password = "testPassword1235%",
            RoleName = "notExistingRole"
        };

        // act
        var action = async () => await _userService.CreateUserAsync(createUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Theory]
    [InlineData("UserName", "")]
    [InlineData("Email", "")]
    [InlineData("Email", "invalidEmail")]
    [InlineData("PhoneNumber", "")]
    [InlineData("Password", "")]
    [InlineData("RoleName", "")]
    public async void CreateUserAsync_InvalidProperty_ThrowsBadRequestException(
        string propertyName, string propertyValue)
    {
        // arrange
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(A<string>.Ignored)).Returns(true);
        A.CallTo(() => _fakeUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

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
        var action = async () => await _userService.CreateUserAsync(createUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void UpdateUserById_RequestIsValid_UpdatesUser()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "testUserName",
            NormalizedUserName = "TESTUSERNAME",
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);
        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);

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
        result.UserName.Should().Be(updateUserRequest.UserName);
        result.NormalizedUserName.Should().Be(updateUserRequest.UserName.ToUpper());
        result.Email.Should().Be(updateUserRequest.Email);
        result.NormalizedEmail.Should().Be(updateUserRequest.Email.ToUpper());
        result.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);
        result.PasswordHash.Should().NotBe(oldPasswordHash);
        result.EmailConfirmed.Should().Be(false);
        result.PhoneNumberConfirmed.Should().Be(false);
        result.PasswordHash.Should().NotBe(oldPasswordHash);
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("UserName", "newUerName")]
    [InlineData("Email", "mail@mail.com")]
    [InlineData("PhoneNumber", "newPhoneNumber")]
    [InlineData("NewPassword", "newPassword")]
    public async void UpdateUserById_SingleFieldInRequest_UpdatesRequestedField(string fieldName, string fieldValue)
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "testUserName",
            NormalizedUserName = "TESTUSERNAME",
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).Returns(IdentityResult.Success);

        var updateUserRequest = new UpdateUserRequest();
        typeof(UpdateUserRequest).GetProperty(fieldName)!.SetValue(updateUserRequest, fieldValue);
        if (updateUserRequest.NewPassword is not null)
        {
            updateUserRequest.ConfirmPassword = updateUserRequest.NewPassword;
        }

        // act
        var result = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        if (updateUserRequest.UserName is not null) result.UserName.Should().Be(updateUserRequest.UserName);
        if (updateUserRequest.UserName is not null)
            result.NormalizedUserName.Should().Be(updateUserRequest.UserName.ToUpper());
        if (updateUserRequest.Email is not null) result.Email.Should().Be(updateUserRequest.Email);
        if (updateUserRequest.Email is not null) result.NormalizedEmail.Should().Be(updateUserRequest.Email.ToUpper());
        if (updateUserRequest.PhoneNumber is not null) result.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);
        if (updateUserRequest.Email is not null) result.EmailConfirmed.Should().Be(true);
        if (updateUserRequest.PhoneNumber is not null) result.PhoneNumberConfirmed.Should().Be(true);
        if (updateUserRequest.NewPassword is not null) result.PasswordHash.Should().NotBe(oldPasswordHash);
        A.CallTo(() => _fakeUserManager.UpdateAsync(A<AppUser>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("UserName", "a")]
    [InlineData("Email", "mailmail.com")]
    public async void UpdateUserById_SingleInvalidField_ThrowsBadRequestException(string fieldName, string fieldValue)
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "testUserName",
            NormalizedUserName = "TESTUSERNAME",
            Email = "mail@mail.com",
            NormalizedEmail = "MAIL@MAIL.COM",
            EmailConfirmed = true,
            PhoneNumber = "123456789",
            PhoneNumberConfirmed = true,
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

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
        var action = async () => await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void UpdateUserById_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeUserManager.Users).Returns(A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock());

        var id = Guid.NewGuid();
        var updateUserRequest = A.Dummy<UpdateUserRequest>();

        // act
        var action = async () => await _userService.UpdateUserByIdAsync(id, updateUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async void UpdateUserById_RequestedUserNameIsAlreadyTaken_ThrowsBadRequestException()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser {Id = Guid.NewGuid(), UserName = "testUserName", PasswordHash = oldPasswordHash};
        var conflictingUser = new AppUser {Id = Guid.NewGuid(), UserName = "conflictingUserName"};
        var usersQueryable = new List<AppUser> {appUser, conflictingUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

        var updateUserRequest = new UpdateUserRequest {UserName = "conflictingUserName"};

        // act
        var action = async () => await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void UpdateUserById_NewPasswordAndConfirmPasswordDontMatch_ThrowsBadRequestException()
    {
        // arrange
        var hasher = new PasswordHasher<AppUser>();
        var oldPasswordHash = hasher.HashPassword(A.Dummy<AppUser>(), "oldPassword");
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(), UserName = "testUserName", NormalizedUserName = "TESTUSERSNAME",
            PasswordHash = oldPasswordHash
        };
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

        var updateUserRequest = new UpdateUserRequest
        {
            NewPassword = "StrongNewPassword12#",
            ConfirmPassword = "NotMatchingNewPassword12#"
        };

        // act
        var action = async () => await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserRequest);

        // assert
        await action.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async void DeleteUserByIdAsync_IdExists_CallsUserManagerDeleteAsyncMethod()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        var id = Guid.NewGuid();
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);

        // act
        await _userService.DeleteUserByIdAsync(id);

        // assert
        A.CallTo(() => _fakeUserManager.DeleteAsync(A<AppUser>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void DeleteUserByIdAsync_IdDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        AppUser appUser = null!;
        var id = Guid.NewGuid();
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var action = async () => await _userService.DeleteUserByIdAsync(id);

        // assert
        await action.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async void DeleteUserByIdAsync_IdDoesntExist_DoesntCallUserManagerDeleteAsyncMethod()
    {
        // arrange
        AppUser appUser = null!;
        var id = Guid.NewGuid();
        A.CallTo(() => _fakeUserManager.FindByIdAsync(A<string>.Ignored)).Returns(appUser);

        // act
        try
        {
            await _userService.DeleteUserByIdAsync(id);
        }
        catch (NotFoundException)
        {
        }

        // assert
        A.CallTo(() => _fakeUserManager.DeleteAsync(A<AppUser>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async void GetUserRolesAsClaims_UserDoesntHaveRoles_DoesntReturnAnyRoleClaim()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserManager.GetRolesAsync(A<AppUser>.Ignored)).Returns(new List<string>());

        // act
        var result = await _userService.GetUserRolesAsClaimsAsync(appUser);

        // assert
        result.Should().NotContain(claim => claim.Type == ClaimTypes.Role);
    }

    [Fact]
    public async void GetUserRolesAsClaims_UserHasRoles_DoesntReturnAnyRoleClaim()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        var roles = new List<string> {"role1", "role2"};
        A.CallTo(() => _fakeUserManager.GetRolesAsync(A<AppUser>.Ignored)).Returns(roles);

        // act
        var result = await _userService.GetUserRolesAsClaimsAsync(appUser);

        // assert
        result.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "role1");
        result.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "role2");
    }

    [Fact]
    public async void UserNameExists_UserNameDoesntExist_ReturnsFalse()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        // act
        var result = await _userService.UserNameExistsAsync("userName");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async void UserNameExists_UserNameExist_ReturnsTrue()
    {
        // arrange
        var fakeAppUserQueryable = new List<AppUser>
        {
            new AppUser {UserName = "userName", NormalizedUserName = "USERNAME"}
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        // act
        var result = await _userService.UserNameExistsAsync("userName");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExists_UserNameExist_ReturnsTrue()
    {
        // arrange
        const string testEmail = "mail@mail.com";
        var fakeAppUserQueryable = new List<AppUser>
        {
            new AppUser
            {
                UserName = "userName",
                NormalizedUserName = "USERNAME",
                Email = testEmail,
                NormalizedEmail = testEmail.ToUpper()
            }
        }.AsQueryable().BuildMock();

        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        // act
        var result = await _userService.EmailExistsAsync(testEmail);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExists_EmailDoesntExist_ReturnsFalse()
    {
        // arrange
        var fakeAppUserQueryable = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(fakeAppUserQueryable);

        // act
        var result = await _userService.EmailExistsAsync("testMail@mail.com");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUserPasswordAsync_ValidPassword_ReturnsTrue()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        const string password = "password";

        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(true);
        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var result = await _userService.ValidateUserPasswordAsync(appUser.UserName, password);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserPasswordAsync_InvalidPassword_ReturnsFalse()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        const string password = "password";

        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(A<AppUser>.Ignored, A<string>.Ignored)).Returns(false);
        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var result = await _userService.ValidateUserPasswordAsync(appUser.UserName, password);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUserPasswordAsync_UserNameDoesntExist_ThrowsNotFoundException()
    {
        // arrange
        AppUser appUser = null!;
        const string password = "password";
        const string nonExistingUserName = "userName";

        A.CallTo(() => _fakeUserManager.FindByNameAsync(A<string>.Ignored)).Returns(appUser);

        // act
        var acton = async () => await _userService.ValidateUserPasswordAsync(nonExistingUserName, password);

        // assert
        await acton.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public void SetUserPassword_ValidUser_SetsUserPassword()
    {
        // arrange
        const string dummyPasswordHash = "dummyPasswordHash";
        var appUser = A.Dummy<AppUser>();
        appUser.PasswordHash = dummyPasswordHash;

        // act
        _userService.SetUserPassword(appUser, "password");

        // assert
        appUser.PasswordHash.Should().NotBe(dummyPasswordHash);
    }

    [Fact]
    public async Task UserExistsById_UserExists_ReturnsTrue()
    {
        // arrange
        var appUser = new AppUser();
        var usersQueryable = new List<AppUser> {appUser}.AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryable);

        // act
        var result = await _userService.UserExistsByIdAsync(appUser.Id);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsById_UserDoesntExist_ReturnsFalse()
    {
        // arrange
        var usersQueryableDummy = A.CollectionOfDummy<AppUser>(0).AsQueryable().BuildMock();
        A.CallTo(() => _fakeUserManager.Users).Returns(usersQueryableDummy);

        // act
        var result = await _userService.UserExistsByIdAsync(Guid.NewGuid());

        // assert
        result.Should().BeFalse();
    }
}