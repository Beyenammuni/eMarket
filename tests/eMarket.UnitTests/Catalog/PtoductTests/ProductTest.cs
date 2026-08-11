using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Events;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using FluentAssertions;
using Xunit;

namespace eMarket.UnitTests.Catalog;

public class ProductTests
{
    [Fact]
    public void Create_Should_Create_Product()
    {
        // Arrange
        var name = ProductName.Create("iPhone 16");
        var description = ProductDescription.Create("Apple smartphone");
        var price = Money.Create(1000m, Currency.USD);
        var sku = Sku.Create("IPHONE-16");
        var categoryId = CategoryId.New();

        // Act
        var businessId = BusinessId.New();

        var result = Product.Create(
            businessId,
            categoryId,
            name,
            description,
            price,
            sku,
            null);

        // Assert

        result.IsSuccess.Should().BeTrue();

        var product = result.Value;
        product.BusinessId.Should().Be(businessId);

        product.ImageUrl.Should().BeNull();

        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.Sku.Should().Be(sku);
        product.CategoryId.Should().Be(categoryId);

        product.StockQuantity.Should().Be(0);
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Rename_Should_Change_Name()
    {
        var product = CreateProduct();

        var newName = ProductName.Create("Galaxy S25");

        var result = product.Rename(newName);

        result.IsSuccess.Should().BeTrue();

        product.Name.Should().Be(newName);
    }

    [Fact]
    public void Rename_Should_Fail_When_Name_Is_Same()
    {
        var product = CreateProduct();

        var result = product.Rename(product.Name);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ChangePrice_Should_Change_Price()
    {
        var product = CreateProduct();

        var newPrice = Money.Create(1500m, Currency.USD);

        var result = product.ChangePrice(newPrice);

        result.IsSuccess.Should().BeTrue();

        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void ChangePrice_Should_Fail_When_Price_Is_Same()
    {
        var product = CreateProduct();

        var result = product.ChangePrice(product.Price);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddStock_Should_Increase_Stock()
    {
        var product = CreateProduct();

        var result = product.IncreaseStock(10);

        result.IsSuccess.Should().BeTrue();

        product.StockQuantity.Should().Be(10);
    }
    [Fact]
    public void ChangeImage_Should_Update_Image()
    {
        var product = CreateProduct();

        var result = product.ChangeImage(
            "https://cdn.test.com/image.png");

        result.IsSuccess.Should().BeTrue();

        product.ImageUrl.Should()
            .Be("https://cdn.test.com/image.png");

        product.UpdatedAt.Should().NotBeNull();
    }
    //[Fact]
    //public void ChangeImage_Should_Raise_DomainEvent()
    //{
    //    var product = CreateProduct();

    //    product.ClearDomainEvents();

    //    product.ChangeImage("image.png");

    //    product.DomainEvents
    //        .Should()
    //        .ContainSingle(x =>
    //            x is ProductImageChangedDomainEvent);
    //}
    [Fact]
    public void Create_Should_Set_BusinessId()
    {
        var businessId = BusinessId.New();

        var result = Product.Create(
            businessId,
            CategoryId.New(),
            ProductName.Create("iPhone"),
            ProductDescription.Create("Phone"),
            Money.Create(1000, Currency.USD),
            Sku.Create("IPHONE"),
            null);

        result.IsSuccess.Should().BeTrue();

        result.Value.BusinessId
            .Should()
            .Be(businessId);
    }
    [Fact]
    public void AddStock_Should_Fail_When_Quantity_Is_Invalid()
    {
        var product = CreateProduct();

        var result = product.IncreaseStock(0);

        result.IsFailure.Should().BeTrue();

        product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void RemoveStock_Should_Decrease_Stock()
    {
        var product = CreateProduct();

        product.IncreaseStock(10);

        var result = product.DecreaseStock(4);

        result.IsSuccess.Should().BeTrue();

        product.StockQuantity.Should().Be(6);
    }

    [Fact]
    public void RemoveStock_Should_Fail_When_Stock_Is_Not_Enough()
    {
        var product = CreateProduct();

        var result = product.DecreaseStock(5);

        result.IsFailure.Should().BeTrue();

        product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void Activate_Should_Change_Status_To_Active()
    {
        var product = CreateProduct();

        var result = product.Activate();

        result.IsSuccess.Should().BeTrue();

        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_Should_Fail_When_Already_Active()
    {
        var product = CreateProduct();

        product.Activate();

        var result = product.Activate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Change_Status_To_Inactive()
    {
        var product = CreateProduct();

        product.Activate();

        var result = product.Deactivate();

        result.IsSuccess.Should().BeTrue();

        product.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void Deactivate_Should_Fail_When_Already_Inactive()
    {
        var product = CreateProduct();

        product.Activate();
        product.Deactivate();

        var result = product.Deactivate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_Raise_ProductCreatedDomainEvent()
    {
        var product = CreateProduct();

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Rename_Should_Raise_ProductRenamedDomainEvent()
    {
        var product = CreateProduct();

        product.ClearDomainEvents();

        product.Rename(ProductName.Create("Galaxy"));

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductRenamedDomainEvent);
    }

    [Fact]
    public void ChangePrice_Should_Raise_ProductPriceChangedDomainEvent()
    {
        var product = CreateProduct();

        product.ClearDomainEvents();

        product.ChangePrice(Money.Create(1500m, Currency.USD));

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductPriceChangedDomainEvent);
    }

    [Fact]
    public void AddStock_Should_Raise_ProductStockIncreasedDomainEvent()
    {
        var product = CreateProduct();

        product.ClearDomainEvents();

        product.IncreaseStock(5);

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductStockIncreasedDomainEvent);
    }

    [Fact]
    public void RemoveStock_Should_Raise_ProductStockDecreasedDomainEvent()
    {
        var product = CreateProduct();

        product.IncreaseStock(10);
        product.ClearDomainEvents();

        product.DecreaseStock(5);

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductStockDecreasedDomainEvent);
    }

    [Fact]
    public void Activate_Should_Raise_ProductActivatedDomainEvent()
    {
        var product = CreateProduct();

        product.ClearDomainEvents();

        product.Activate();

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductActivatedDomainEvent);
    }

    [Fact]
    public void Deactivate_Should_Raise_ProductDeactivatedDomainEvent()
    {
        var product = CreateProduct();

        product.Activate();
        product.ClearDomainEvents();

        product.Deactivate();

        product.DomainEvents
            .Should()
            .ContainSingle(x => x is ProductDeactivatedDomainEvent);
    }

    private static Product? CreateProduct()
    {
        return Product.Create(
            BusinessId.New(),
            CategoryId.New(),
            ProductName.Create("iPhone 16"),
            ProductDescription.Create("Apple smartphone"),
            Money.Create(1000m, Currency.USD),
            Sku.Create("IPHONE-16"),
            null)
            .Value;
    }
}
