using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanPayments;

public sealed record GetSellerPlanPaymentsQuery
    : IRequest<IReadOnlyList<SellerPlanPaymentDto>>;

public sealed record SellerPlanPaymentDto(
    Guid Id,
    Guid BusinessId,
    string BusinessName,
    Guid SellerPlanId,
    string PlanName,
    Guid SellerPlanSubscriptionId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? VerifiedAt
);
