using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.Events;
using eMarket.Domain.Sales.Orders.ValueObjects;
using FluentAssertions;
using Xunit;

namespace eMarket.UnitTests.Sales.Orders;

public class OrderTests
{
    [Fact]
    public void Create_Should_Create_Order()
    {
        var userId = UserId.New();
        var businessId = BusinessId.New();

        var addressResult = DeliveryAddress.Create(
            "John Doe",
            "123-456-7890",
            "123 Main St",
            "New York",
            "Manhattan",
            "10001",
            "Downtown",
            "Main St",
            "B1",
            "Apt 2",
            40.7128m,
            -74.0060m);

        addressResult.IsSuccess.Should().BeTrue();

        var result = Order.Create(
            userId,
            businessId,
            addressResult.Value!);

        result.IsSuccess.Should().BeTrue();

        var order = result.Value!;

        order.UserId.Should().Be(userId);
        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void Create_Should_Raise_OrderCreatedDomainEvent()
    {
        var order = CreateOrder();

        order.DomainEvents
            .Should()
            .ContainSingle(x => x is OrderCreatedDomainEvent);
    }

    [Fact]
    public void AddItem_Should_Add_Item()
    {
        var order = CreateOrder();

        var result = order.AddItem(
            ProductId.New(),
            "iPhone 16",
            Money.Create(3000m, Currency.TRY),
            2);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(6000m);
    }

    [Fact]
    public void AddItem_Should_Fail_When_Quantity_Is_Invalid()
    {
        var order = CreateOrder();

        var result = order.AddItem(
            ProductId.New(),
            "iPhone 16",
            Money.Create(3000m, Currency.TRY),
            0);

        result.IsFailure.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsPaid_Should_Change_Status()
    {
        var order = CreateOrder();

        order.AddItem(
            ProductId.New(),
            "iPhone 16",
            Money.Create(3000m, Currency.TRY),
            1);

        var result = order.MarkAsPaid();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_Should_Fail_When_Order_Is_Empty()
    {
        var order = CreateOrder();

        var result = order.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public void Cancel_Should_Change_Status()
    {
        var order = CreateOrder();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Should_Fail_When_Order_Is_Shipped()
    {
        var order = CreateOrderWithItem();

        order.MarkAsPaid();
        order.StartProcessing();
        order.Ship();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void StartProcessing_Should_Change_Status()
    {
        var order = CreateOrderWithItem();

        order.MarkAsPaid();

        var result = order.StartProcessing();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Ship_Should_Change_Status()
    {
        var order = CreateOrderWithItem();

        order.MarkAsPaid();
        order.StartProcessing();

        var result = order.Ship();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Deliver_Should_Change_Status()
    {
        var order = CreateOrderWithItem();

        order.MarkAsPaid();
        order.StartProcessing();
        order.Ship();

        var result = order.Deliver();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Cancel_Should_Raise_DomainEvent()
    {
        var order = CreateOrder();

        order.ClearDomainEvents();
        order.Cancel();

        order.DomainEvents
            .Should()
            .ContainSingle(x => x is OrderCancelledDomainEvent);
    }

    private static Order CreateOrder()
    {
        var addressResult = DeliveryAddress.Create(
            "John Doe",
            "123-456-7890",
            "123 Main St",
            "New York",
            "Manhattan",
            "10001",
            "Downtown",
            "Main St",
            "B1",
            "Apt 2",
            40.7128m,
            -74.0060m);

        addressResult.IsSuccess.Should().BeTrue();

        var orderResult = Order.Create(
            UserId.New(),
           BusinessId.New(),
            addressResult.Value!);

        orderResult.IsSuccess.Should().BeTrue();

        return orderResult.Value!;
    }

    private static Order CreateOrderWithItem()
    {
        var order = CreateOrder();

        order.AddItem(
            ProductId.New(),
            "iPhone 16",
            Money.Create(3000m, Currency.TRY),
            1);

        return order;
    }
}
