using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Services;

public sealed class SellerPlanFeatureService(
    IBusinessDbContext dbContext) : ISellerPlanFeatureService
{
    public async Task<bool> HasFeatureAsync(
        Guid businessId,
        string featureCode,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty || string.IsNullOrWhiteSpace(featureCode))
            return false;

        var now = DateTime.UtcNow;
        var id = BusinessId.Create(businessId);

        return await (
            from subscription in dbContext.SellerPlanSubscriptions
            join feature in dbContext.SellerPlanFeatures
                on subscription.SellerPlanId equals feature.SellerPlanId
            where subscription.BusinessId == id
                  && subscription.Status == "Active"
                  && subscription.StartedAt <= now
                  && subscription.ExpiresAt > now
                  && feature.Code == featureCode
            select subscription.Id
        ).AnyAsync(cancellationToken);
    }
}
