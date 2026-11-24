using eMarket.Domain.Promotions.Enums;

namespace eMarket.Application.Promotions.Commands.CreatePromotion;

public sealed record CreatePromotionResponse(
    Guid PromotionId,
    string Name,
    string Status,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<CreatePromotionItemResponse> Items);

public sealed record CreatePromotionItemResponse(
    Guid ProductId,
    DiscountType DiscountType,
    decimal DiscountValue);
