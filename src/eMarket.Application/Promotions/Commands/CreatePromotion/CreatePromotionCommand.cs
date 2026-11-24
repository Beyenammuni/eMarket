using eMarket.Domain.Promotions.Enums;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Commands.CreatePromotion;

public sealed record CreatePromotionItemRequest(
    Guid ProductId,
    DiscountType DiscountType,
    decimal DiscountValue);

public sealed record CreatePromotionCommand(
    string Name,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<CreatePromotionItemRequest> Items)
    : IRequest<Result<CreatePromotionResponse>>;
