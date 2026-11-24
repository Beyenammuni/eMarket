using eMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanSubscriptions;

public sealed class GetSellerPlanSubscriptionsQueryHandler
    : IRequestHandler<
        GetSellerPlanSubscriptionsQuery,
        IReadOnlyList<SellerPlanSubscriptionDto>>
{
    private readonly IBusinessDbContext _dbContext;

    public GetSellerPlanSubscriptionsQueryHandler(
        IBusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SellerPlanSubscriptionDto>> Handle(
        GetSellerPlanSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await (
            from subscription in _dbContext.SellerPlanSubscriptions.AsNoTracking()
            join business in _dbContext.Businesses.AsNoTracking()
                on subscription.BusinessId equals business.Id
            join plan in _dbContext.SellerPlans.AsNoTracking()
                on subscription.SellerPlanId equals plan.Id
            orderby subscription.CreatedAt descending
            select new SellerPlanSubscriptionDto(
                subscription.Id,
                subscription.BusinessId.Value,
                business.Name.Value,
                plan.Id,
                plan.Name,
                plan.Price,
                plan.Currency,
                subscription.Status,
                subscription.StartedAt,
                subscription.ExpiresAt,
                subscription.CreatedAt
            )
        ).ToListAsync(cancellationToken);
    }
}
