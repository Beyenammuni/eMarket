using eMarket.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetCurrentSellerPlanSubscription;

public sealed class GetCurrentSellerPlanSubscriptionQueryHandler(
    IBusinessDbContext dbContext)
    : IRequestHandler<
        GetCurrentSellerPlanSubscriptionQuery,
        CurrentSellerPlanSubscriptionDto?>
{
    public async Task<CurrentSellerPlanSubscriptionDto?> Handle(
        GetCurrentSellerPlanSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var subscription = await (
            from s in dbContext.SellerPlanSubscriptions
            join p in dbContext.SellerPlans
                on s.SellerPlanId equals p.Id
            where s.BusinessId == Domain.Businesses.BusinessId.Create(request.BusinessId)
                  && s.Status == "Active"
                  && s.StartedAt <= now
                  && s.ExpiresAt > now
            orderby p.Price descending, s.StartedAt descending
            select new
            {
                Subscription = s,
                Plan = p
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
            return null;

        return new CurrentSellerPlanSubscriptionDto(
            subscription.Subscription.Id,
            subscription.Plan.Id,
            subscription.Plan.Code,
            subscription.Plan.Name,
            subscription.Plan.Price,
            subscription.Plan.Currency,
            subscription.Subscription.Status,
            subscription.Subscription.StartedAt,
            subscription.Subscription.ExpiresAt,
            subscription.Plan.Price > 0,
            true);
    }
}
