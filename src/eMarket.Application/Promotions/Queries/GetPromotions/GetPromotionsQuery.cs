
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Queries.GetPromotions;

public sealed record GetPromotionsQuery
    : IRequest<Result<IReadOnlyCollection<GetPromotionResponse>>>;
