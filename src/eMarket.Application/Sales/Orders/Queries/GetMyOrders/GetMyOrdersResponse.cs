namespace eMarket.Application.Sales.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersResponse(
    Guid Id,
    string Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    int TotalItems);
