using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Subscriptions.Entities;
using eMarket.Domain.Subscriptions.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Subscriptions;

public sealed class Subscription : AggregateRoot<SubscriptionId>
{
    private readonly List<SubscriptionItem> _items = [];
    private Subscription() { }

    private Subscription(SubscriptionId id, UserId userId, BusinessId businessId, DayOfWeek deliveryDay, DateTime nextDeliveryDate, SubscriptionDeliveryAddress deliveryAddress)
    {
        Id = id;
        UserId = userId;
        BusinessId = businessId;
        DeliveryDay = deliveryDay;
        DeliveryAddress = deliveryAddress;
        NextDeliveryDate = nextDeliveryDate.Date;
        Status = SubscriptionStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public UserId UserId { get; private set; } = default!;
    public BusinessId BusinessId { get; private set; } = default!;
    public SubscriptionDeliveryAddress DeliveryAddress { get; private set; } = default!;
    public DayOfWeek DeliveryDay { get; private set; }
    public DateTime NextDeliveryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public DateTime? CancelledAt { get; private set; }
    public Guid? LastGeneratedOrderId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public IReadOnlyCollection<SubscriptionItem> Items => _items.AsReadOnly();



    public static Result<Subscription>
        Create(
    UserId userId,
    BusinessId businessId,
    DayOfWeek deliveryDay,
    DateTime nextDeliveryDate,
    SubscriptionDeliveryAddress deliveryAddress)
    {
        var subscription = new Subscription(SubscriptionId.New(),
            userId,
            businessId,
            deliveryDay,
            nextDeliveryDate, deliveryAddress);
        return Result<Subscription>.Success(subscription);
    }

    public Result AddOrUpdateItem(ProductId productId, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(SubscriptionErrors.InvalidQuantity);
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) _items.Add(
            new SubscriptionItem(productId, quantity));
        else item.ChangeQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveItem(ProductId productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return Result.Success();
        _items.Remove(item);
        if (_items.Count == 0)
            return Result.Failure(SubscriptionErrors.EmptyBasket);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ReplaceItems(IEnumerable<(ProductId ProductId, int Quantity)> items)
    {
        var materialized = items.ToList();
        if (materialized.Count == 0)
            return Result.Failure(SubscriptionErrors.EmptyBasket);
        if (materialized.Any(x => x.Quantity <= 0))
            return Result.Failure(SubscriptionErrors.InvalidQuantity);
        _items.Clear();
        foreach (var item in materialized) _items.Add(new SubscriptionItem(item.ProductId, item.Quantity));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetDeliveryDay(DayOfWeek deliveryDay)
    {
        DeliveryDay = deliveryDay;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SkipNextDelivery()
    {
        if (Status != SubscriptionStatus.Active) return Result.Failure(SubscriptionErrors.NotActive);
        NextDeliveryDate = NextDeliveryDate.AddDays(7);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure(SubscriptionErrors.AlreadyCancelled);
        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RecordOrderGenerated(Guid orderId)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.NotActive);
        LastGeneratedOrderId = orderId;
        NextDeliveryDate = NextDeliveryDate.AddDays(7);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
