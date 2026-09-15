using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Orders.Entities;
using eMarket.Domain.Sales.Orders.Events;
using eMarket.Domain.Sales.Orders.Rules;
using eMarket.Domain.Sales.Orders.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Sales.Orders;

public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];

private Order()
    {
    }

    private Order(
        OrderId id,
        UserId userId,
        BusinessId businessId)
    {
        Id = id;
        UserId = userId;
        BusinessId = businessId;
        Status = OrderStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;
    }

    public UserId UserId { get; private set; } = default!;

    public BusinessId BusinessId { get; private set; } = default!;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public DeliveryAddress DeliveryAddress { get; private set; } = default!;

    public IReadOnlyCollection<OrderItem> Items =>
        _items.AsReadOnly();

    public decimal TotalAmount =>
        _items.Sum(x => x.TotalPrice);

    public static Result<Order> Create(
        UserId userId,
        BusinessId businessId,
        DeliveryAddress deliveryAddress)
    {
        var order = new Order(
            OrderId.New(),
            userId,
            businessId);

        order.DeliveryAddress = deliveryAddress;

        order.AddDomainEvent(
            new OrderCreatedDomainEvent(order.Id));

        return Result<Order>.Success(order);
    }

    public Result AddItem(
        ProductId productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(
                CartErrors.InvalidQuantity);
        }

        var item = new OrderItem(
            productId,
            productName,
            unitPrice,
            quantity);

        _items.Add(item);

        return Result.Success();
    }

    public Result MarkAsPaid()
    {
        if (new OrderCanBePaidRule(Status).IsBroken())
        {
            return Result.Failure(
                OrderErrors.InvalidStatus);
        }

        if (new OrderMustHaveItemsRule(Items).IsBroken())
        {
            return Result.Failure(
                OrderErrors.EmptyOrder);
        }

        Status = OrderStatus.Paid;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new OrderPaidDomainEvent(Id));

        return Result.Success();
    }

    public Result StartProcessing()
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Failure(
                OrderErrors.InvalidStatus);
        }

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SetDeliveryAddress(
        DeliveryAddress deliveryAddress)
    {
        DeliveryAddress = deliveryAddress;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != OrderStatus.Processing)
        {
            return Result.Failure(
                OrderErrors.InvalidStatus);
        }

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Deliver()
    {
        if (Status != OrderStatus.Shipped)
        {
            return Result.Failure(
                OrderErrors.InvalidStatus);
        }

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is
            OrderStatus.Shipped or
            OrderStatus.Delivered or
            OrderStatus.Cancelled)
        {
            return Result.Failure(
                OrderErrors.CannotCancel);
        }

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new OrderCancelledDomainEvent(Id));

        return Result.Success();
    }

}
