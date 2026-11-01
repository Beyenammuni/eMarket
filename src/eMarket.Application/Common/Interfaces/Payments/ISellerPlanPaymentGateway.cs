
namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record SellerPlanPaymentRequest(
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string ReturnUrl,
    string CallbackUrl,
    string BuyerId,
    string BuyerName,
    string BuyerSurname,
    string BuyerEmail,
    string BuyerPhone,
    string BuyerIdentityNumber);

public interface ISellerPlanPaymentGateway
{
    Task<PaymentGatewayResult> CreatePaymentAsync(
        SellerPlanPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentCheckoutResult> GetCheckoutResultAsync(
        string token,
        CancellationToken cancellationToken = default);
}
