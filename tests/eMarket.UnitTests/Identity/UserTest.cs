using FluentAssertions;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.ValueObjects;
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
}
