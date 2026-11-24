using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Queries.GetActivePromotions;

public sealed record GetActivePromotionsQuery
    : IRequest<Result<IReadOnlyCollection<GetActivePromotionResponse>>>;

public sealed record GetActivePromotionResponse(
    Guid PromotionId,
    string PromotionName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<ActivePromotionProductResponse> Items);

public sealed record ActivePromotionProductResponse(
    Guid ProductId,
    Guid BusinessId,
    string ProductName,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    string Currency,
    string? ImageUrl,
    int DiscountType,
    decimal DiscountValue);
