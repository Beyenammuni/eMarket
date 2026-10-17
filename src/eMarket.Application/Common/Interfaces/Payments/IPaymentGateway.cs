using eMarket.Application.Common.Interfaces.Payments;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentCheckoutResult> GetCheckoutResultAsync(
        string token,
        CancellationToken cancellationToken = default);
}
