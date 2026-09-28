namespace eMarket.Application.Common.Interfaces.Payments;

public interface ISubscriptionPaymentGateway
{
    Task<SubscriptionPaymentGatewayResult> CreateCheckoutFormAsync(
        SubscriptionPaymentGatewayRequest request,
        CancellationToken cancellationToken = default);

    Task<string> TestListProductsAsync(
    CancellationToken cancellationToken = default);
}

public sealed record SubscriptionPaymentGatewayRequest(
    Guid PaymentId,
    decimal MonthlyAmount,
    string Currency,
    string ReturnUrl,
    SubscriptionPaymentBuyer Buyer,
    SubscriptionPaymentAddress Address);

public sealed record SubscriptionPaymentBuyer(
    string Name,
    string Surname,
    string Email,
    string Phone,
    string IdentityNumber);

public sealed record SubscriptionPaymentAddress(
    string FullName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string PostalCode,
    string Neighborhood,
    string Street,
    string BuildingNumber,
    string ApartmentNumber);

public sealed record SubscriptionPaymentGatewayResult(
    bool IsSuccess,
    string? CheckoutFormContent,
    string? Token,
    string? Error);
