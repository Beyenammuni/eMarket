using FluentAssertions;
using eMarket.Domain.Catalog.Products.ValueObjects;
using Xunit;

namespace eMarket.UnitTests.Catalog.ValueObjects;

public class ProductNameTests
{
    [Fact]
    public void Create_Should_Create_ProductName()
    {
        // Act
        var name = ProductName.Create("iPhone 16");

        // Assert
        name.Value.Should().Be("iPhone 16");
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Empty()
    {
        // Arrange
        var action = () => ProductName.Create("");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Two_ProductNames_With_Same_Value_Should_Be_Equal()
    {
        // Arrange
        var first = ProductName.Create("Galaxy S25");
        var second = ProductName.Create("Galaxy S25");

        // Assert
        first.Should().Be(second);
    }
}
