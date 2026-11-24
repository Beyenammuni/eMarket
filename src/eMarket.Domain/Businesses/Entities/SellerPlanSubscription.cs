
namespace eMarket.Domain.Businesses.Entities;

public sealed class SellerPlanSubscription
{
    private SellerPlanSubscription()
    {
    }

    public Guid Id { get; private set; }

    public BusinessId BusinessId { get; private set; } = default!;
    public Guid SellerPlanId { get; private set; }

    public string Status { get; private set; } = "Pending";

    public DateTime StartedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string? ProviderPaymentId { get; private set; }

    public string? PaymentToken { get; private set; }

    public static SellerPlanSubscription CreatePending(
        BusinessId businessId,
        Guid sellerPlanId)
    {
        if (businessId == default)
            throw new ArgumentException("BusinessId is required.");

        if (sellerPlanId == Guid.Empty)
            throw new ArgumentException("SellerPlanId is required.");

        return new SellerPlanSubscription
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            SellerPlanId = sellerPlanId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate(
    DateTime startedAt,
    int durationDays,
    string? providerPaymentId)
    {
        if (durationDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationDays));

        if (string.IsNullOrWhiteSpace(providerPaymentId))
            throw new ArgumentException("Verified payment ID is required.");

        StartedAt = startedAt;
        ExpiresAt = startedAt.AddDays(durationDays);
        ProviderPaymentId = providerPaymentId;
        Status = "Active";
    }
    public void ActivateFree()
    {
        if (Status != "Pending")
            throw new InvalidOperationException(
                "Only pending subscriptions can be activated.");

        StartedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.MaxValue;
        ProviderPaymentId = null;
        PaymentToken = null;
        Status = "Active";
    }
    public void MarkAsFailed()
    {
        if (Status == "Active")
            throw new InvalidOperationException(
                "An active subscription cannot be marked as failed.");

        Status = "Failed";
    }

    public bool IsActiveAt(DateTime utcNow) =>
        Status == "Active" &&
        StartedAt <= utcNow &&
        ExpiresAt > utcNow;
}
