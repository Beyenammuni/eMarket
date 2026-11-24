using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanSubscriptions;

public sealed record GetSellerPlanSubscriptionsQuery
    : IRequest<IReadOnlyList<SellerPlanSubscriptionDto>>;

public sealed record SellerPlanSubscriptionDto(
    Guid Id,
    Guid BusinessId,
    string BusinessName,
    Guid SellerPlanId,
    string PlanName,
    decimal PlanPrice,
    string Currency,
    string Status,
    DateTime StartedAt,
    DateTime ExpiresAt,
    DateTime CreatedAt
);
