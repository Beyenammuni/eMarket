namespace eMarket.Application.Sales.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdResponse(
    Guid Id,
    Guid UserId,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    decimal TotalAmount,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal TotalPrice);
