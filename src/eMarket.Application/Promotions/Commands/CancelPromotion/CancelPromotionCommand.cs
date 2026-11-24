
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Commands.CancelPromotion;

public sealed record CancelPromotionCommand(Guid PromotionId)
    : IRequest<Result<CancelPromotionResponse>>;

public sealed record CancelPromotionResponse(
    Guid PromotionId,
    string Status);
