
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Commands.PausePromotion;

public sealed record PausePromotionCommand(Guid PromotionId)
    : IRequest<Result<PausePromotionResponse>>;

public sealed record PausePromotionResponse(
    Guid PromotionId,
    string Status);
