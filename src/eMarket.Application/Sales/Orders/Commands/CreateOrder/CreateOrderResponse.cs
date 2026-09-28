namespace eMarket.Application.Sales.Orders.Commands.CreateOrder;

public sealed record CreateOrderResponse(
    Guid Id,
    decimal TotalAmount,
    decimal ShippingFee,
    string Status,
    int TotalItems);
