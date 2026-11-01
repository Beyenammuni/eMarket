namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record PaymentCheckoutResult(
    bool IsSuccess,
    string? PaymentId,
    string? Error,
    string? ConversationId,
    string? BasketId,
    string? Price,
    string? PaidPrice,
    string? Currency);
