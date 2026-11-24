
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Commands.ActivatePromotion;

public sealed record ActivatePromotionCommand(Guid PromotionId)
    : IRequest<Result<ActivatePromotionResponse>>;

public sealed record ActivatePromotionResponse(
    Guid PromotionId,
    string Status);
