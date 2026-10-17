using eMarket.Domain.Common;

namespace eMarket.Application.Sales.Payments.Queries.GetSubscriptionPayment;

public sealed record GetSubscriptionPaymentResponse(
    Guid PaymentId,
    Guid SubscriptionId,
    decimal Amount,
    Currency Currency,
    string Status,
    string? Provider,
    string? ProviderPaymentId,
    DateTime CreatedAt);
