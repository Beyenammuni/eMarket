using eMarket.Domain.Common;

namespace eMarket.Application.Sales.Carts.Queries.GetCart;

public sealed record GetCartResponse(
    Guid Id,
    int TotalItems,
    decimal TotalAmount,
    List<CartItemResponse> Items);

public sealed record CartItemResponse(
    Guid ProductId,
    decimal UnitPrice,
    Currency Currency,
    int Quantity,
    decimal TotalPrice);
