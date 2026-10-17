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
        CurrentPeriodStart = DateTime.UtcNow.Date;

        CurrentPeriodEnd = CurrentPeriodStart.AddMonths(1);
        BillingCycle = SubscriptionBillingCycle.Monthly;
        Status = SubscriptionStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;

    }

    public UserId UserId { get; private set; } = default!;
    public BusinessId BusinessId { get; private set; } = default!;
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public SubscriptionDeliveryAddress DeliveryAddress { get; private set; } = default!;
    public SubscriptionBillingCycle BillingCycle { get; private set; }
    public DayOfWeek DeliveryDay { get; private set; }
    public DateTime NextDeliveryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public DateTime? CancelledAt { get; private set; }
    public Guid? LastGeneratedOrderId { get; private set; }
    public string? IyzicoSubscriptionReferenceCode { get; private set; }
    public decimal WeeklyProductAmount { get; private set; }

    public decimal WeeklyShippingAmount { get; private set; }

    public decimal WeeklyTotalAmount { get; private set; }

    public decimal MonthlyAmount { get; private set; }
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
            nextDeliveryDate,
            deliveryAddress);
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

    if (item is null)
        return Result.Success();

    if (_items.Count == 1)
        return Result.Failure(SubscriptionErrors.EmptyBasket);

    _items.Remove(item);
    UpdatedAt = DateTime.UtcNow;

    return Result.Success();
}



    public Result ReplaceItems(
        IEnumerable<(ProductId ProductId, int Quantity)> items)
    {
        var materialized = items.ToList();

        if (materialized.Count == 0)
            return Result.Failure(SubscriptionErrors.EmptyBasket);

        if (materialized.Any(x => x.Quantity <= 0))
            return Result.Failure(SubscriptionErrors.InvalidQuantity);

        if (materialized
            .GroupBy(x => x.ProductId)
            .Any(group => group.Count() > 1))
        {
            return Result.Failure(
                new Error(
                    "Subscription.DuplicateProduct",
                    "A product cannot appear more than once."));
        }

        _items.Clear();

        foreach (var item in materialized)
        {
            _items.Add(
                new SubscriptionItem(item.ProductId, item.Quantity));
        }

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
    public Result SetPricing(
    decimal weeklyProductAmount,
    decimal weeklyShippingAmount)
    {
        if (weeklyProductAmount <= 0)
            return Result.Failure(
                new Error(
                    "Subscription.InvalidProductAmount",
                    "Weekly product amount must be greater than zero."));

        if (weeklyShippingAmount < 0)
            return Result.Failure(
                new Error(
                    "Subscription.InvalidShippingAmount",
                    "Weekly shipping amount cannot be negative."));

        WeeklyProductAmount = Math.Round(
            weeklyProductAmount,
            2,
            MidpointRounding.AwayFromZero);

        WeeklyShippingAmount = Math.Round(
            weeklyShippingAmount,
            2,
            MidpointRounding.AwayFromZero);

        WeeklyTotalAmount = Math.Round(
            WeeklyProductAmount + WeeklyShippingAmount,
            2,
            MidpointRounding.AwayFromZero);

        MonthlyAmount = Math.Round(
            WeeklyTotalAmount * 52m / 12m,
            2,
            MidpointRounding.AwayFromZero);

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SetIyzicoSubscriptionReference(
     string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            return Result.Failure(
                SubscriptionErrors.NotActive);

        IyzicoSubscriptionReferenceCode =
            referenceCode.Trim();

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
    public Result Activate()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure(
                SubscriptionErrors.AlreadyCancelled);

        if (Status != SubscriptionStatus.PendingPayment)
            return Result.Failure(
                new Error(
                    "Subscription.InvalidActivation",
                    "Only subscriptions pending payment can be activated."));

        Status = SubscriptionStatus.Active;

        CurrentPeriodStart = DateTime.UtcNow.Date;
        CurrentPeriodEnd = CurrentPeriodStart.AddMonths(1);

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
