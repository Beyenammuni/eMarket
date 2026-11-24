
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Seeders;

public sealed class SellerPlanSeeder(IBusinessDbContext context)
{
    private static readonly string[] ProFeatures =
    [
        SellerPlanFeatureCodes.Dashboard.AdvancedAnalytics,
        SellerPlanFeatureCodes.Dashboard.TopProducts,
        SellerPlanFeatureCodes.Dashboard.ExtendedPeriod,
        SellerPlanFeatureCodes.Reports.AdvancedSales,
        SellerPlanFeatureCodes.Reports.StorePerformance,
        SellerPlanFeatureCodes.Orders.AdvancedAnalytics,
        SellerPlanFeatureCodes.Inventory.AdvancedManagement,
        SellerPlanFeatureCodes.Products.AdvancedManagement,
        SellerPlanFeatureCodes.Staff.AdvancedManagement,
        SellerPlanFeatureCodes.Branches.Management,
        SellerPlanFeatureCodes.Subscriptions.Management
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var plans = new[]
        {
            ("FREE", "Ücretsiz", 0m, 0),
            ("PRO_MONTHLY", "Pazora Pro Aylık", 499m, 30),
            ("PRO_ANNUAL", "Pazora Pro Yıllık", 4990m, 365)
        };

        foreach (var plan in plans)
        {
            var exists = await context.SellerPlans
                .AnyAsync(x => x.Code == plan.Item1, cancellationToken);

            if (!exists)
            {
                context.SellerPlans.Add(
                    SellerPlan.Create(
                        plan.Item1,
                        plan.Item2,
                        plan.Item3,
                        plan.Item4));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var proPlans = await context.SellerPlans
            .Where(x => x.Code == "PRO_MONTHLY" || x.Code == "PRO_ANNUAL")
            .ToListAsync(cancellationToken);

        foreach (var plan in proPlans)
        {
            var existingCodes = await context.SellerPlanFeatures
                .Where(x => x.SellerPlanId == plan.Id)
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            var codes = existingCodes.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

            foreach (var featureCode in ProFeatures)
            {
                if (codes.Add(featureCode))
                {
                    context.SellerPlanFeatures.Add(
                        SellerPlanFeature.Create(plan.Id, featureCode));
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
