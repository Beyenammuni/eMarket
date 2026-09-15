using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.Events;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using FluentAssertions;

namespace eMarket.UnitTests.Catalog.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_Should_Create_Category()
    {
        var name = CategoryName.Create("Electronics");

        var result = Category.Create(name);

        result.IsSuccess.Should().BeTrue();

        result.Value.Name.Should().Be(name);

        result.Value.Status.Should().Be(CategoryStatus.Active);
    }

    [Fact]
    public void Rename_Should_Change_Name()
    {
        var category = Category.Create(
            CategoryName.Create("Electronics"))
            .Value;

        var newName = CategoryName.Create("Phones");

        var result = category.Rename(newName);

        result.IsSuccess.Should().BeTrue();

        category.Name.Should().Be(newName);
    }

    [Fact]
    public void Rename_Should_Fail_When_Name_Is_Same()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        var result = category.Rename(
            CategoryName.Create("Books"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Change_Status()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        category.Deactivate();

        category.Status.Should().Be(CategoryStatus.Inactive);
    }

    [Fact]
    public void Activate_Should_Change_Status()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        category.Deactivate();

        category.Activate();

        category.Status.Should().Be(CategoryStatus.Active);
    }
    [Fact]
    public void Rename_Should_Update_Name()
    {
        var category = Category
            .Create(CategoryName.Create("Electronics"))
            .Value;

        var result = category.Rename(
            CategoryName.Create("Phones"));

        result.IsSuccess.Should().BeTrue();

        category.Name.Value.Should().Be("Phones");
    }
    [Fact]
    public void Rename_Should_Return_Failure_When_Name_Is_Same()
    {
        var category = Category
            .Create(CategoryName.Create("Electronics"))
            .Value;

        var result = category.Rename(
            CategoryName.Create("Electronics"));

        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(CategoryErrors.SameName);
    }
    [Fact]
    public void Rename_Should_Add_Domain_Event()
    {
        var category = Category
            .Create(CategoryName.Create("Electronics"))
            .Value;

        category.ClearDomainEvents();

        category.Rename(
            CategoryName.Create("Phones"));

        category.DomainEvents.Should().ContainSingle();

        category.DomainEvents.First()
            .Should()
            .BeOfType<CategoryRenamedDomainEvent>();
    }
    [Fact]
    public void Deactivate_Should_Set_Status_To_Inactive()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        category.Deactivate();

        category.Status.Should().Be(CategoryStatus.Inactive);
    }
    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        category.Deactivate();

        category.Activate();

        category.Status.Should().Be(CategoryStatus.Active);
    }
    [Fact]
    public void Deactivate_Should_Add_Domain_Event()
    {
        var category = Category.Create(
            CategoryName.Create("Books"))
            .Value;

        category.ClearDomainEvents();

        category.Deactivate();

        category.DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<CategoryDeactivatedDomainEvent>();
    }
}
