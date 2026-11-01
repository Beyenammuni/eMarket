
using eMarket.Domain.Businesses;

namespace eMarket.Domain.Businesses.Entities;

public sealed class SellerPlanPayment
{
    private SellerPlanPayment() { }

    public Guid Id { get; private set; }
    public BusinessId BusinessId { get; private set; } = default!;
    public Guid SellerPlanId { get; private set; }
    public Guid SellerPlanSubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public string Status { get; private set; } = "Pending";
    public string? ProviderToken { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    public static SellerPlanPayment Create(
        BusinessId businessId,
        Guid sellerPlanId,
        Guid sellerPlanSubscriptionId,
        decimal amount,
        string currency)
    {
        if (businessId is null || businessId.Value == Guid.Empty)
            throw new ArgumentException("BusinessId is required.");

        if (sellerPlanId == Guid.Empty ||
            sellerPlanSubscriptionId == Guid.Empty)
            throw new ArgumentException("Plan and subscription IDs are required.");

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("A valid currency is required.");

        return new SellerPlanPayment
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            SellerPlanId = sellerPlanId,
            SellerPlanSubscriptionId = sellerPlanSubscriptionId,
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetProviderToken(string token)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Payment is not pending.");

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Provider token is required.");

        ProviderToken = token;
    }

    public void MarkAsPaid(string providerPaymentId)
    {
        if (Status == "Paid")
            return;

        if (Status != "Pending")
            throw new InvalidOperationException("Payment is not pending.");

        if (string.IsNullOrWhiteSpace(providerPaymentId))
            throw new ArgumentException("Verified provider payment ID is required.");

        ProviderPaymentId = providerPaymentId;
        Status = "Paid";
        VerifiedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == "Paid")
            throw new InvalidOperationException("A paid payment cannot be marked as failed.");

        Status = "Failed";
    }
}
