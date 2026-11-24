
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanFeatures;

public sealed record GetSellerPlanFeaturesQuery(Guid BusinessId)
    : IRequest<SellerPlanFeaturesDto>;
