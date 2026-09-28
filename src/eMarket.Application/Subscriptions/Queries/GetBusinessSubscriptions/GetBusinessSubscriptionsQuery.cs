using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Subscriptions.Queries.GetBusinessSubscriptions;

public sealed record GetBusinessSubscriptionsQuery(
    Guid BusinessId)
    : IRequest<Result<IReadOnlyCollection<GetBusinessSubscriptionResponse>>>;

public sealed record GetBusinessSubscriptionResponse(
    Guid Id,
    Guid BusinessId,
    Guid UserId,
    string CustomerName,
    string CustomerUsername,
    DayOfWeek DeliveryDay,
    DateTime NextDeliveryDate,
    string Status,
    Guid? LastGeneratedOrderId,
    SubscriptionDeliveryAddressResponse DeliveryAddress,
    IReadOnlyCollection<SubscriptionItemResponse> Items);

public sealed record SubscriptionDeliveryAddressResponse(
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

public sealed record SubscriptionItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity);
