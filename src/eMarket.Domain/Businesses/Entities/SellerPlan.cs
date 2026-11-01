
namespace eMarket.Domain.Businesses.Entities;

public sealed class SellerPlan
{
    private SellerPlan()
    {
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public string Currency { get; private set; } = "TRY";

    public int DurationDays { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static SellerPlan Create(
        string code,
        string name,
        decimal price,
        int durationDays,
        string currency = "TRY")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Plan code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.");

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        if (durationDays < 0)
            throw new ArgumentOutOfRangeException(nameof(durationDays));

        return new SellerPlan
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Price = price,
            Currency = currency,
            DurationDays = durationDays,
            IsActive = true,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        decimal price,
        int durationDays,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.");

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        if (durationDays < 0)
            throw new ArgumentOutOfRangeException(nameof(durationDays));

        Name = name.Trim();
        Price = price;
        DurationDays = durationDays;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
