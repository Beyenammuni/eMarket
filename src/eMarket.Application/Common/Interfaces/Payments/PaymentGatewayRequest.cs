namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record PaymentGatewayRequest(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string ReturnUrl);
