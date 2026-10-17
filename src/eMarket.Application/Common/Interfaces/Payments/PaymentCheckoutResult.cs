namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record PaymentCheckoutResult(
    bool IsSuccess,
    string? PaymentId,
    string? Error);
