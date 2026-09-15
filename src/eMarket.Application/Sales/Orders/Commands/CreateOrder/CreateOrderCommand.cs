using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
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
    decimal Longitude)
    : IRequest<Result<CreateOrderResponse>>;
