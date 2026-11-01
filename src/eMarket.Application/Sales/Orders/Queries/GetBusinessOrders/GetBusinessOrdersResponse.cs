namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrders;

public sealed record GetBusinessOrdersResponse(
    Guid Id,
    string CustomerName,
    string CustomerUsername,
    string Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    int TotalItems,
    List<BusinessOrderItemResponse> Items);

public sealed record BusinessOrderItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
