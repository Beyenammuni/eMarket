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
        BusinessId businessId,
        BranchId branchId)
    {
        Id = id;
        UserId = userId;
        BusinessId = businessId;
        Status = OrderStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;
        BranchId = branchId;
    }

    public UserId UserId { get; private set; } = default!;

    public BusinessId BusinessId { get; private set; } = default!;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
    public decimal ShippingFee { get; private set; }
    public bool StockReleased { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } = default!;
    public BranchId? BranchId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items =>
        _items.AsReadOnly();

    public decimal TotalAmount =>
    _items.Sum(x => x.TotalPrice) + ShippingFee;

    public static Result<Order> Create(
     UserId userId,
     BusinessId businessId,
     BranchId branchId,
     DeliveryAddress deliveryAddress,
     decimal shippingFee)
    {
        var order = new Order(
            OrderId.New(),
            userId,
            businessId,
            branchId);

        order.DeliveryAddress = deliveryAddress;
        order.ShippingFee = shippingFee;

        order.AddDomainEvent(
            new OrderCreatedDomainEvent(order.Id));

        return Result<Order>.Success(order);
    }



    public Result AddItem(
        ProductId productId,
        string productName,
        Money originalUnitPrice,
        Money discountAmount,
        Money unitPrice,
        int quantity,
        Guid? variantId = null,
        string? variantName = null,
        Guid? promotionId = null,
        string? promotionName = null)
    {
        if (quantity <= 0)
        {
            return Result.Failure(
                CartErrors.InvalidQuantity);
        }

        var item = new OrderItem(
            productId,
            productName,
            originalUnitPrice,
            discountAmount,
            unitPrice,
            quantity,
            variantId,
            variantName,
            promotionId,
            promotionName);

        _items.Add(item);

        return Result.Success();
    }



    public Result MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
        {
            return Result.Success();
        }

        if (new OrderCanBePaidRule(Status).IsBroken())
        {
            return Result.Failure(OrderErrors.InvalidStatus);
        }

        if (new OrderMustHaveItemsRule(Items).IsBroken())
        {
            return Result.Failure(OrderErrors.EmptyOrder);
        }

        Status = OrderStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, UserId, Status));
        AddDomainEvent(new OrderPaidDomainEvent(Id));

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
        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, UserId, Status));
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
        AddDomainEvent(new OrderCancelledDomainEvent(Id));

        return Result.Success();
    }
    public Result ReleaseStock()
    {
        if (StockReleased)
            return Result.Success();

        if (Status is OrderStatus.Paid
            or OrderStatus.Processing
            or OrderStatus.Shipped
            or OrderStatus.Delivered)
        {
            return Result.Failure(OrderErrors.InvalidStatus);
        }

        StockReleased = true;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
    public Result MarkStockReleased()
    {
        if (StockReleased)
            return Result.Success();

        if (Status != OrderStatus.Cancelled)
            return Result.Failure(OrderErrors.InvalidStatus);

        StockReleased = true;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

}
