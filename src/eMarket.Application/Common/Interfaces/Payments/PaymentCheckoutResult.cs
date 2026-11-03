namespace eMarket.Application.Common.Interfaces.Payments;

public enum PaymentVerificationStatus
{
    Unknown = 0,
    Succeeded = 1,
    Failed = 2,
    Pending = 3
}

public sealed record PaymentCheckoutResult(
    bool IsSuccess,
    string? PaymentId,
    string? Error,
    string? ConversationId,
    string? BasketId,
    string? Price,
    string? PaidPrice,
    string? Currency)
{
    public PaymentVerificationStatus VerificationStatus { get; init; }
        = PaymentVerificationStatus.Unknown;
    public int? FraudStatus { get; init; }
}
