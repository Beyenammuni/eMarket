
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerPlanFeatures;

public sealed class GetSellerPlanFeaturesQueryHandler(
    ISellerPlanFeatureService featureService)
    : IRequestHandler<GetSellerPlanFeaturesQuery, SellerPlanFeaturesDto>
{
    public async Task<SellerPlanFeaturesDto> Handle(
        GetSellerPlanFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var businessId = request.BusinessId;

        var dashboardAdvancedAnalytics = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Dashboard.AdvancedAnalytics,
            cancellationToken);

        var dashboardTopProducts = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Dashboard.TopProducts,
            cancellationToken);

        var dashboardExtendedPeriod = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Dashboard.ExtendedPeriod,
            cancellationToken);

        var reportsAdvancedSales = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Reports.AdvancedSales,
            cancellationToken);

        var reportsStorePerformance = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Reports.StorePerformance,
            cancellationToken);

        var ordersAdvancedAnalytics = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Orders.AdvancedAnalytics,
            cancellationToken);

        var inventoryAdvancedManagement = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Inventory.AdvancedManagement,
            cancellationToken);
        var productsAdvancedManagement = await featureService.HasFeatureAsync(
    businessId,
    SellerPlanFeatureCodes.Products.AdvancedManagement,
    cancellationToken);

        var staffAdvancedManagement = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Staff.AdvancedManagement,
            cancellationToken);

        var branchesManagement = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Branches.Management,
            cancellationToken);

        var subscriptionsManagement = await featureService.HasFeatureAsync(
            businessId,
            SellerPlanFeatureCodes.Subscriptions.Management,
            cancellationToken);

        return new SellerPlanFeaturesDto(
            Dashboard: new DashboardFeaturesDto(
                dashboardAdvancedAnalytics,
                dashboardTopProducts,
                dashboardExtendedPeriod),
            Reports: new ReportsFeaturesDto(
                reportsAdvancedSales,
                reportsStorePerformance),
            Orders: new OrdersFeaturesDto(ordersAdvancedAnalytics),
            Inventory: new InventoryFeaturesDto(inventoryAdvancedManagement),
            Products: new ProductsFeaturesDto(productsAdvancedManagement),
            Staff: new StaffFeaturesDto(staffAdvancedManagement),
            Branches: new BranchesFeaturesDto(branchesManagement),
            Subscriptions: new SubscriptionsFeaturesDto(subscriptionsManagement));
    }
}
