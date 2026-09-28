namespace eMarket.Application.Common.Interfaces.Payments;

public sealed record PaymentGatewayRequest(
    Guid PaymentId,
    Guid? OrderId,
    Guid? SubscriptionId,
    PaymentGatewayPaymentType PaymentType,
    decimal Amount,
    string Currency,
    string ReturnUrl,
    PaymentBuyer Buyer,
    PaymentGatewayMerchant Merchant,
    PaymentGatewayAddress DeliveryAddress,
    IReadOnlyCollection<PaymentGatewayItem> Items);

public enum PaymentGatewayPaymentType
{
    Order = 1,
    Subscription = 2
}

public sealed record PaymentBuyer(
    Guid Id,
    string Name,
    string Surname,
    string Email,
    string Phone);

public sealed record PaymentGatewayMerchant(
    string SubMerchantKey,
    decimal PlatformCommissionRate);

public sealed record PaymentGatewayAddress(
    string FullName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string PostalCode,
    string Neighborhood,
    string Street,
    string BuildingNumber,
    string ApartmentNumber,
    decimal Latitude,
    decimal Longitude);

public sealed record PaymentGatewayItem(
    Guid ProductId,
    string Name,
    decimal Price,
    int Quantity);
