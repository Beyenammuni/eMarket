using eMarket.Domain.Common;

public sealed record CreateProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Currency Currency,
    string Sku,
    string? ImageUrl);
