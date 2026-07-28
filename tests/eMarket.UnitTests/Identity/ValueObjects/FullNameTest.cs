using FluentAssertions;
using eMarket.Domain.Identity.ValueObjects;

namespace eMarket.UnitTests.Identity.ValueObjects;

public class FullNameTests
{
    [Fact]
    public void Create_Should_Create_FullName()
    {
        var fullName = FullName.Create("Bayan", "Amounh");

        fullName.FirstName.Should().Be("Bayan");
        fullName.LastName.Should().Be("Amounh");
        fullName.Full.Should().Be("Bayan Amounh");
    }

    [Fact]
    public void Create_Should_Throw_When_FirstName_Is_Empty()
    {
        Action action = () => FullName.Create("", "Amounh");

        action.Should().Throw<ArgumentException>();
    }
}
