using FluentAssertions;
using eMarket.Domain.Catalog.ValueObjects;
using Xunit;

namespace eMarket.UnitTests.Catalog.ValueObjects;

public class CategoryNameTests
{
    [Fact]
    public void Create_Should_Create_CategoryName()
    {
        var name = CategoryName.Create("Electronics");

        name.Value.Should().Be("Electronics");
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Empty()
    {
        var action = () => CategoryName.Create("");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Two_CategoryNames_With_Same_Value_Should_Be_Equal()
    {
        var first = CategoryName.Create("Books");
        var second = CategoryName.Create("Books");

        first.Should().Be(second);
    }
}
