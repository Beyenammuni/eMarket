namespace eMarket.Application.Sales.Payments.Commands.CreatePayment;

public sealed record CreatePaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentUrl);
