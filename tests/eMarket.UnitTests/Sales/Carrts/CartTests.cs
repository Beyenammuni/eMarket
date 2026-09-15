using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Carts.Events;
using eMarket.Domain.Sales.Carts.ValueObjects;
using FluentAssertions;
using Xunit;

namespace eMarket.UnitTests.Sales.Carts;

public class CartTests
{
    [Fact]
    public void Create_Should_Create_Cart()
    {
        var userId = UserId.New();

        var result = Cart.Create(userId);

        result.IsSuccess.Should().BeTrue();

        var cart = result.Value;

        cart.UserId.Should().Be(userId);
        cart.Status.Should().Be(CartStatus.Active);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Raise_CartCreatedDomainEvent()
    {
        var cart = CreateCart();

        cart.DomainEvents
            .Should()
            .ContainSingle(
                x => x is CartCreatedDomainEvent);
    }

    [Fact]
    public void AddItem_Should_Add_Item()
    {
        var cart = CreateCart();

        var productId = ProductId.New();

        var result = cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        result.IsSuccess.Should().BeTrue();

        cart.Items.Should().ContainSingle();

        cart.Items.First()
            .ProductId
            .Should()
            .Be(productId);

        cart.Items.First()
            .Quantity.Value
            .Should()
            .Be(2);
    }

    [Fact]
    public void AddItem_Should_Raise_DomainEvent()
    {
        var cart = CreateCart();

        cart.ClearDomainEvents();

        var productId = ProductId.New();

        var result = cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        result.IsSuccess.Should().BeTrue();

        cart.DomainEvents
            .Should()
            .ContainSingle(
                x => x is CartItemAddedDomainEvent);
    }

    [Fact]
    public void AddItem_Should_Fail_When_Quantity_Is_Invalid()
    {
        var cart = CreateCart();

        var act = () => cart.AddItem(
            ProductId.New(),
            Money.Create(100m, Currency.TRY),
            Quantity.Create(0),
            10);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void RemoveItem_Should_Remove_Item()
    {
        var cart = CreateCart();

        var productId = ProductId.New();

        cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        var result = cart.RemoveItem(productId);

        result.IsSuccess.Should().BeTrue();

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_Should_Raise_DomainEvent()
    {
        var cart = CreateCart();

        var productId = ProductId.New();

        cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        cart.ClearDomainEvents();

        var result = cart.RemoveItem(productId);

        result.IsSuccess.Should().BeTrue();

        cart.DomainEvents
            .Should()
            .ContainSingle(
                x => x is CartItemRemovedDomainEvent);
    }

    [Fact]
    public void RemoveItem_Should_Fail_When_Product_Not_Found()
    {
        var cart = CreateCart();

        var result = cart.RemoveItem(
            ProductId.New());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ChangeItemQuantity_Should_Change_Quantity()
    {
        var cart = CreateCart();

        var productId = ProductId.New();

        cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        var result = cart.ChangeItemQuantity(
            productId,
            Quantity.Create(5));

        result.IsSuccess.Should().BeTrue();

        cart.Items.First()
            .Quantity.Value
            .Should()
            .Be(5);
    }

    [Fact]
    public void ChangeItemQuantity_Should_Raise_DomainEvent()
    {
        var cart = CreateCart();

        var productId = ProductId.New();

        cart.AddItem(
            productId,
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        cart.ClearDomainEvents();

        var result = cart.ChangeItemQuantity(
            productId,
            Quantity.Create(5));

        result.IsSuccess.Should().BeTrue();

        cart.DomainEvents
            .Should()
            .ContainSingle(
                x => x is CartItemQuantityChangedDomainEvent);
    }

    [Fact]
    public void Clear_Should_Remove_All_Items()
    {
        var cart = CreateCart();

        cart.AddItem(
            ProductId.New(),
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        cart.AddItem(
            ProductId.New(),
            Money.Create(200m, Currency.TRY),
            Quantity.Create(1), 10);

        var result = cart.Clear();

        result.IsSuccess.Should().BeTrue();

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_Should_Raise_DomainEvent()
    {
        var cart = CreateCart();

        cart.AddItem(
            ProductId.New(),
            Money.Create(100m, Currency.TRY),
            Quantity.Create(2), 10);

        cart.ClearDomainEvents();

        var result = cart.Clear();

        result.IsSuccess.Should().BeTrue();

        cart.DomainEvents
            .Should()
            .ContainSingle(
                x => x is CartClearedDomainEvent);
    }

    [Fact]
    public void Clear_Should_Fail_When_Cart_Is_Empty()
    {
        var cart = CreateCart();

        var result = cart.Clear();

        result.IsFailure.Should().BeTrue();
    }

    private static Cart CreateCart()
    {
        return Cart.Create(
            UserId.New()).Value;
    }
}
