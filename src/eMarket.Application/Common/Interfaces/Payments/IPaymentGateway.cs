namespace eMarket.Application.Common.Interfaces.Payments;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);
}
