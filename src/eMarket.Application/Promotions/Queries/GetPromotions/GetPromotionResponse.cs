
using eMarket.Domain.Promotions.Enums;

namespace eMarket.Application.Promotions.Queries.GetPromotions;

public sealed record GetPromotionResponse(
    Guid PromotionId,
    string Name,
    string? Description,
    string Status,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    DateTime CreatedAt,
    IReadOnlyCollection<GetPromotionItemResponse> Items);

public sealed record GetPromotionItemResponse(
    Guid ProductId,
    string ProductName,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    string Currency,
    DiscountType DiscountType,
    decimal DiscountValue);
