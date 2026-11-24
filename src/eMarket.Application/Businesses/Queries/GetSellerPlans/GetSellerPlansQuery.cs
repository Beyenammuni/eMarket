using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlans;

public sealed record GetSellerPlansQuery(
    bool ActiveOnly = false
) : IRequest<IReadOnlyList<SellerPlanDto>>;

public sealed record SellerPlanDto(
    Guid Id,
    string Code,
    string Name,
    decimal Price,
    string Currency,
    int DurationDays,
    bool IsActive,
    DateTime UpdatedAt,
    IReadOnlyList<string> Features
);
