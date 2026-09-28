using eMarket.Application.Sales.Orders.Queries.GetOrderById;

public sealed record GetBusinessOrderByIdResponse(
    Guid Id,
    Guid UserId,
    string CustomerName,
    string CustomerUsername,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    decimal TotalAmount,
    DeliveryAddressResponse DeliveryAddress,
    List<OrderItemResponse> Items);

public sealed record DeliveryAddressResponse(
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
