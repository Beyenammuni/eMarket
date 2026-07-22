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

}
