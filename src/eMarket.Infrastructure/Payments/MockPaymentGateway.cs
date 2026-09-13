using eMarket.Application.Common.Interfaces.Payments;

namespace eMarket.Infrastructure.Payments;

public sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        var providerPaymentId =
            $"MOCK-{Guid.NewGuid():N}";

        var paymentUrl =
            $"http://localhost:7172/checkout/{providerPaymentId}";

        return Task.FromResult(
            new PaymentGatewayResult(
                true,
                paymentUrl,
                providerPaymentId,
                null));
    }
}
