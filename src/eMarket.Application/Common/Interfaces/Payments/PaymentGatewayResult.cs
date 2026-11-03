namespace eMarket.Application.Common.Interfaces.Payments;

public enum PaymentCreationStatus
{
    Unknown = 0,
    Succeeded = 1,
    Failed = 2
}

public sealed record PaymentGatewayResult(
    bool IsSuccess,
    string? PaymentUrl,
    string? ProviderPaymentId,
    string? Error)
{
    public string? CheckoutToken { get; init; }

    public PaymentCreationStatus CreationStatus { get; init; }
        = PaymentCreationStatus.Unknown;
}
