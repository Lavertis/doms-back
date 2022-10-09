using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Identity;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DoctorsOffice.UnitTests;

public class AppUserManagerTests : UnitTest
{
    private readonly AppUserManager _appUserManager;
    private readonly IPasswordHasher<AppUser> _fakePasswordHasher;
    private readonly IUserStore<AppUser> _fakeUserStore;

    public AppUserManagerTests()
    {
        _fakeUserStore = A.Fake<IUserStore<AppUser>>(options => options
            .Implements<IUserEmailStore<AppUser>>()
            .Implements<IUserPasswordStore<AppUser>>()
        );
        _fakePasswordHasher = A.Fake<IPasswordHasher<AppUser>>();
        _appUserManager = new AppUserManager(
            _fakeUserStore,
            null!,
            _fakePasswordHasher,
            null!,
            null!,
            null!,
            null!,
            null!,
            A.Dummy<ILogger<AppUserManager>>()
        );
    }

    [Fact]
    public async void FindByIdAsync_IdDoesntExist_ReturnsFailedResultWithoutUser()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.FindByIdAsync(Guid.NewGuid());

        // assert
        result.IsError.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async void FindByIdAsync_IdExists_ReturnsUser()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.FindByIdAsync(Guid.NewGuid());

        // assert
        result.Value.Should().Be(appUser);
    }

    [Fact]
    public async void FindByNameAsync_UserNameExists_ReturnsUserWithSuccess()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserStore.FindByNameAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(appUser);

        // act
        var result = await _appUserManager.FindByNameAsync("test");

        // assert
        result.Value.Should().Be(appUser);
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async void FindByNameAsync_UserNameDoesntExist_ReturnsFailedResultWithoutUser()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserStore.FindByNameAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(appUser);

        // act
        var result = await _appUserManager.FindByNameAsync("test");

        // assert
        result.IsError.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async void DeleteByIdAsync_IdExists_CallsUserManagerDeleteAsyncMethod()
    {
        // arrange
        A.CallTo(() => _fakeUserStore.DeleteAsync(A<AppUser>.Ignored, A<CancellationToken>.Ignored))
            .Returns(IdentityResult.Success);

        // act
        await _appUserManager.DeleteByIdAsync(Guid.NewGuid());

        // assert
        A.CallTo(() => _fakeUserStore.DeleteAsync(A<AppUser>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void DeleteByIdAsync_IdDoesntExist_ReturnsFailedResultWithFalseValue()
    {
        // arrange
        AppUser appUser = null!;
        var id = Guid.NewGuid();
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.DeleteByIdAsync(id);

        // assert
        result.IsError.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async void DeleteByIdAsync_IdDoesntExist_DoesntCallUserManagerDeleteAsyncMethod()
    {
        // arrange
        AppUser appUser = null!;
        var id = Guid.NewGuid();
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        await _appUserManager.DeleteByIdAsync(id);

        // assert
        A.CallTo(() => _fakeUserStore.DeleteAsync(A<AppUser>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async void ExistsByUserNameAsync_UserNameDoesntExist_ReturnsFalse()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserStore.FindByNameAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByUserNameAsync("userName");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async void ExistsByUserNameAsync_UserNameExist_ReturnsTrue()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserStore.FindByNameAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByUserNameAsync("userName");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_UserNameExist_ReturnsTrue()
    {
        // arrange
        const string testEmail = "mail@mail.com";
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() =>
            ((IUserEmailStore<AppUser>) _fakeUserStore).FindByEmailAsync(A<string>.Ignored,
                A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByEmailAsync(testEmail);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_EmailDoesntExist_ReturnsFalse()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() =>
            ((IUserEmailStore<AppUser>) _fakeUserStore).FindByEmailAsync(A<string>.Ignored,
                A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByEmailAsync("testMail@mail.com");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePasswordAsync_ValidPassword_ReturnsTrue()
    {
        // arrange
        A.CallTo(() =>
                _fakePasswordHasher.VerifyHashedPassword(A<AppUser>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .Returns(PasswordVerificationResult.Success);

        // act
        var result = await _appUserManager.ValidatePasswordAsync("userName", "password");

        // assert
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_InvalidPassword_ReturnsFalse()
    {
        // arrange
        A.CallTo(() =>
                _fakePasswordHasher.VerifyHashedPassword(A<AppUser>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .Returns(PasswordVerificationResult.Failed);

        // act
        var result = await _appUserManager.ValidatePasswordAsync("userName", "password");

        // assert
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePasswordAsync_UserNameDoesntExist_ReturnsFailedResult()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserStore.FindByNameAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(appUser);

        // act
        var result = await _appUserManager.ValidatePasswordAsync("userName", "password");

        // assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByIdAsync_UserExists_ReturnsTrue()
    {
        // arrange
        var appUser = A.Dummy<AppUser>();
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByIdAsync(appUser.Id);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByIdAsync_UserDoesntExist_ReturnsFalse()
    {
        // arrange
        AppUser appUser = null!;
        A.CallTo(() => _fakeUserStore.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(appUser);

        // act
        var result = await _appUserManager.ExistsByIdAsync(Guid.NewGuid());

        // assert
        result.Should().BeFalse();
    }
}