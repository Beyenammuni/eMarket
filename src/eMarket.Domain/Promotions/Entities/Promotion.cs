
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Promotions.Entities;

public sealed class Promotion : AggregateRoot<PromotionId>
{
    private readonly List<PromotionItem> _items = [];

    private Promotion()
    {
    }

    private Promotion(
        PromotionId id,
        BusinessId businessId,
        string name,
        string? description,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        Id = id;
        BusinessId = businessId;
        Name = name;
        Description = description;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = PromotionStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public BusinessId BusinessId { get; private set; } = default!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public PromotionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<PromotionItem> Items =>
        _items.AsReadOnly();

    public static Promotion Create(
        BusinessId businessId,
        string name,
        string? description,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        ArgumentNullException.ThrowIfNull(businessId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Promotion name is required.", nameof(name));

        name = name.Trim();

        if (name.Length > 150)
            throw new ArgumentException(
                "Promotion name cannot exceed 150 characters.",
                nameof(name));

        if (description?.Length > 1000)
            throw new ArgumentException(
                "Description cannot exceed 1000 characters.",
                nameof(description));

        if (startsAtUtc.Kind != DateTimeKind.Utc ||
            endsAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Promotion dates must be UTC.");
        }

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException(
                "End date must be after start date.");

        return new Promotion(
            PromotionId.New(),
            businessId,
            name,
            string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            startsAtUtc,
            endsAtUtc);
    }

    public void AddItem(PromotionItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Status is PromotionStatus.Active
            or PromotionStatus.Completed
            or PromotionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Items cannot be modified in the current promotion status.");
        }

        if (item.PromotionId != Id)
            throw new InvalidOperationException(
                "Promotion item belongs to another promotion.");

        if (_items.Any(x => x.ProductId == item.ProductId))
            throw new InvalidOperationException(
                "Product already exists in this promotion.");

        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(ProductId productId)
    {
        ArgumentNullException.ThrowIfNull(productId);

        if (Status is PromotionStatus.Active
            or PromotionStatus.Completed
            or PromotionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Items cannot be modified in the current promotion status.");
        }

        var item = _items.FirstOrDefault(
            x => x.ProductId == productId);

        if (item is null)
            throw new InvalidOperationException(
                "Product does not exist in this promotion.");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status is PromotionStatus.Cancelled
            or PromotionStatus.Completed)
        {
            throw new InvalidOperationException(
                "Promotion cannot be activated.");
        }

        if (_items.Count == 0)
            throw new InvalidOperationException(
                "Promotion must contain at least one product.");

        var now = DateTime.UtcNow;

        if (now < StartsAtUtc)
            Status = PromotionStatus.Scheduled;
        else if (now < EndsAtUtc)
            Status = PromotionStatus.Active;
        else
            throw new InvalidOperationException(
                "Promotion has already expired.");

        UpdatedAt = now;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        EnsureCanBeEdited();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Promotion name is required.");

        name = name.Trim();

        if (name.Length > 150)
            throw new ArgumentException(
                "Promotion name cannot exceed 150 characters.");

        if (description?.Length > 1000)
            throw new ArgumentException(
                "Description cannot exceed 1000 characters.");

        if (startsAtUtc.Kind != DateTimeKind.Utc ||
            endsAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Promotion dates must be UTC.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException(
                "End date must be after start date.");

        Name = name;
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceItems(IEnumerable<PromotionItem> newItems)
    {
        EnsureCanBeEdited();
        ArgumentNullException.ThrowIfNull(newItems);

        var incomingItems = newItems.ToList();

        if (incomingItems.Count == 0)
            throw new InvalidOperationException(
                "Promotion must contain at least one product.");

        if (incomingItems.Any(x => x.PromotionId != Id))
            throw new InvalidOperationException(
                "Promotion item belongs to another promotion.");

        if (incomingItems.Select(x => x.ProductId).Distinct().Count()
            != incomingItems.Count)
            throw new InvalidOperationException(
                "Duplicate products are not allowed.");

        var incomingProductIds = incomingItems
            .Select(x => x.ProductId)
            .ToHashSet();

        // Remove products no longer included in the promotion.
        _items.RemoveAll(existing =>
            !incomingProductIds.Contains(existing.ProductId));

        foreach (var incoming in incomingItems)
        {
            var existing = _items.FirstOrDefault(
                x => x.ProductId == incoming.ProductId);

            if (existing is not null)
            {
                // Preserve the existing database row and update its discount.
                existing.UpdateDiscount(
                    incoming.DiscountType,
                    incoming.DiscountValue);
            }
            else
            {
                // Add only genuinely new products.
                _items.Add(incoming);
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureCanBeEdited()
    {
        if (Status is PromotionStatus.Active
            or PromotionStatus.Completed
            or PromotionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Promotion cannot be edited in its current status.");
        }
    }


    public void Pause()
    {
        if (Status is not PromotionStatus.Active
            and not PromotionStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Only active or scheduled promotions can be paused.");
        }

        Status = PromotionStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is PromotionStatus.Completed
            or PromotionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Promotion cannot be cancelled.");
        }

        Status = PromotionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status is PromotionStatus.Cancelled
            or PromotionStatus.Completed)
        {
            throw new InvalidOperationException(
                "Promotion cannot be completed.");
        }

        var now = DateTime.UtcNow;

        if (now < EndsAtUtc)
            throw new InvalidOperationException(
                "Promotion has not ended yet.");

        Status = PromotionStatus.Completed;
        UpdatedAt = now;
    }

    public bool IsCurrentlyActive(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new ArgumentException(
                "Date must be UTC.", nameof(utcNow));

        var isEnabled =
            Status == PromotionStatus.Active ||
            Status == PromotionStatus.Scheduled;

        return isEnabled
            && utcNow >= StartsAtUtc
            && utcNow < EndsAtUtc;
    }
}
