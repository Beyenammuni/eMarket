using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Events;
using eMarket.Domain.Identity.ValueObjects;
using FluentAssertions;
using Xunit;

namespace eMarket.UnitTests.Domain.Identity;

public class UserTests
{
    [Fact]
    public void Register_Should_Create_User_With_Default_Values()
    {
        // Arrange
        var fullName = FullName.Create("Bayan", "Amounh");
        var email = Email.Create("bayan@test.com");
        var phone = PhoneNumber.Create("+905551112233");

        // Act
        var result = User.Register(fullName, email, phone);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var user = result.Value;

        user.FullName.Should().Be(fullName);
        user.Email.Should().Be(email);
        user.PhoneNumber.Should().Be(phone);

        user.Status.Should().Be(UserStatus.Pending);

        user.EmailVerified.Should().BeFalse();

        user.Roles.Should().Contain(UserRole.Customer);
    }
    [Fact]
    public void AssignRole_Should_Fail_When_Role_Already_Exists()
    {
        // Arrange
        var fullName = FullName.Create("Bayan", "Amounh");
        var email = Email.Create("bayan@test.com");
        var phone = PhoneNumber.Create("+905551112233");

        var user = User.Register(fullName, email, phone).Value;

        // Act
        var result = user.AssignRole(UserRole.Customer);
        // Assert
        result.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void AssignRole_Should_Add_New_Role()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.AssignRole(UserRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.Roles.Should().Contain(UserRole.Admin);
        user.Roles.Should().HaveCount(2);
    }
    [Fact]
    public void RemoveRole_Should_Remove_Role()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.AssignRole(UserRole.Admin);

        // Act
        var result = user.RemoveRole(UserRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.Roles.Should().NotContain(UserRole.Admin);
        user.Roles.Should().Contain(UserRole.Customer);
    }
    [Fact]
    public void RemoveRole_Should_Fail_When_Removing_Last_Role()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.RemoveRole(UserRole.Customer);

        // Assert
        result.IsFailure.Should().BeTrue();

        user.Roles.Should().ContainSingle();
    }
    [Fact]
    public void RecordLogin_Should_Update_LastLoginAt()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }
    [Fact]
    public void Activate_Should_Return_Success_When_Already_Active()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.Activate();

        // Act
        var result = user.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }
    [Fact]
    public void Suspend_Should_Set_Status_To_Suspended()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.Suspend();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Suspended);
    }
    [Fact]
    public void Suspend_Should_Return_Success_When_Already_Suspended()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.Suspend();

        // Act
        var result = user.Suspend();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Suspended);
    }
    [Fact]
    public void Register_Should_Raise_UserRegisteredDomainEvent()
    {
        // Arrange
        var fullName = FullName.Create("Bayan", "Amounh");
        var email = Email.Create("bayan@test.com");
        var phone = PhoneNumber.Create("+905551112233");

        // Act
        var user = User.Register(fullName, email, phone).Value;

        // Assert
        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserRegisteredDomainEvent);
    }
    [Fact]
    public void AssignRole_Should_Raise_UserRoleAssignedDomainEvent()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.ClearDomainEvents();

        // Act
        user.AssignRole(UserRole.Admin);

        // Assert
        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserRoleAssignedDomainEvent);
    }
    [Fact]
    public void VerifyEmail_Should_Raise_UserEmailVerifiedDomainEvent()
    {
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.ClearDomainEvents();

        user.VerifyEmail();

        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserEmailVerifiedDomainEvent);
    }
    [Fact]
    public void ChangeEmail_Should_Raise_UserEmailChangedDomainEvent()
    {
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.ClearDomainEvents();

        user.ChangeEmail(Email.Create("new@test.com"));

        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserEmailChangedDomainEvent);
    }
    [Fact]
    public void Activate_Should_Raise_UserActivatedDomainEvent()
    {
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.ClearDomainEvents();

        user.Activate();

        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserActivatedDomainEvent);
    }
    [Fact]
    public void Suspend_Should_Raise_UserSuspendedDomainEvent()
    {
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.ClearDomainEvents();

        user.Suspend();

        user.DomainEvents
            .Should()
            .ContainSingle(e => e is UserSuspendedDomainEvent);
    }
}
