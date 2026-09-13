namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record PaymentGatewayResult(
    bool IsSuccess,
    string? PaymentUrl,
    string? ProviderPaymentId,
    string? Error);
