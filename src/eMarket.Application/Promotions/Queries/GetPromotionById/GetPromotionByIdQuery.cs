
using eMarket.Application.Promotions.Queries.GetPromotions;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Queries.GetPromotionById;

public sealed record GetPromotionByIdQuery(Guid PromotionId)
    : IRequest<Result<GetPromotionResponse>>;
