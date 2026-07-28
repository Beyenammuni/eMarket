using eMarket.Domain.Identity;
using eMarket.Domain.Identity.ValueObjects;
using FluentAssertions;

namespace eMarket.UnitTests.Identity.ValueObject;

public class EmailTest
{
    [Fact]
    public void Create_Should_Return_Email_When_Value_Is_Valid()
    {
        // Arrange
        const string validEmail = "test@example.com";
        // Act
        var email = Email.Create(validEmail);
        // Assert
        email.Value.Should().Be(validEmail);
    }
    [Fact]
    public void Create_Should_Throw_Exception_When_Value_Is_Empty()
    {
        // Arrange
        const string emptyEmail = "";
        // Act
        Action act = () => Email.Create(emptyEmail);
        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Email cannot be empty.");
    }
    [Fact]
    public void Create_Should_Throw_Exception_When_Value_Is_Invalid()
    {
        // Arrange
        const string invalidEmail = "invalid-email";
        // Act
        Action act = () => Email.Create(invalidEmail);
        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid email format.");
    }
    [Fact]
    public void Two_Emails_With_Same_Value_Should_Be_Equal() {
        string first = Email.Create("test@gmail.com");
        string second = Email.Create("test@gmail.com");

        first.Should().Be(second);
    }
    [Fact]
    public void Create_Should_Normalize_Email_To_LowerCase()
    {
        // Arrange
        const string value = "BAYAN@TEST.COM";

        // Act
        var email = Email.Create(value);

        // Assert
        email.Value.Should().Be("bayan@test.com");
    }
    [Fact]
    public void VerifyEmail_Should_Set_EmailVerified()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.VerifyEmail();

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.EmailVerified.Should().BeTrue();
    }
    [Fact]
    public void VerifyEmail_Should_Fail_When_Already_Verified()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        user.VerifyEmail();

        // Act
        var result = user.VerifyEmail();

        // Assert
        result.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void ChangeEmail_Should_Update_Email()
    {
        // Arrange
        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            Email.Create("bayan@test.com"),
            PhoneNumber.Create("+905551112233"))
            .Value;

        var newEmail = Email.Create("new@test.com");

        // Act
        var result = user.ChangeEmail(newEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.Email.Should().Be(newEmail);
    }
    [Fact]
    public void ChangeEmail_Should_Fail_When_Email_Is_Same()
    {
        // Arrange
        var email = Email.Create("bayan@test.com");

        var user = User.Register(
            FullName.Create("Bayan", "Amounh"),
            email,
            PhoneNumber.Create("+905551112233"))
            .Value;

        // Act
        var result = user.ChangeEmail(email);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
