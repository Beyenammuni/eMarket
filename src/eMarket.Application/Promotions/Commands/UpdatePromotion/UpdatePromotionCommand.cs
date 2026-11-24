
using eMarket.Domain.Promotions.Enums;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Commands.UpdatePromotion;

public sealed record UpdatePromotionItemRequest(
    Guid ProductId,
    DiscountType DiscountType,
    decimal DiscountValue);

public sealed record UpdatePromotionCommand(
    Guid PromotionId,
    string Name,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<UpdatePromotionItemRequest> Items)
    : IRequest<Result<UpdatePromotionResponse>>;

public sealed record UpdatePromotionResponse(
    Guid PromotionId,
    string Name,
    string Status,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<UpdatePromotionItemResponse> Items);

public sealed record UpdatePromotionItemResponse(
    Guid ProductId,
    DiscountType DiscountType,
    decimal DiscountValue);
