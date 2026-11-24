using MediatR;

namespace eMarket.Application.Businesses.Queries.GetCurrentSellerPlanSubscription;

public sealed record GetCurrentSellerPlanSubscriptionQuery(
    Guid BusinessId
) : IRequest<CurrentSellerPlanSubscriptionDto?>;
