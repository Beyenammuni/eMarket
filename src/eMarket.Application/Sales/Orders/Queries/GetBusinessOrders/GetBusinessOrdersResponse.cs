namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrders;

public sealed record GetBusinessOrdersResponse(
    Guid Id,
    string Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    int TotalItems);
