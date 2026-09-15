using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    Guid BusinessId,
    DayOfWeek DeliveryDay,
    DateTime FirstDeliveryDate,
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
    decimal Longitude,
    IReadOnlyCollection<SubscriptionItemRequest> Items)
    : IRequest<Result<CreateSubscriptionResponse>>;

public sealed record SubscriptionItemRequest(
    Guid ProductId,
    int Quantity);
