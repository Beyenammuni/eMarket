
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerDashboard;

internal sealed class GetSellerDashboardQueryHandler(
    ISellerDashboardRepository repository,
    ISellerPlanFeatureService featureService)
    : IRequestHandler<
        GetSellerDashboardQuery,
        Result<SellerDashboardResponse>>
{
    public async Task<Result<SellerDashboardResponse>> Handle(
        GetSellerDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BusinessId is null ||
            request.BusinessId == Guid.Empty)
        {
            return Result<SellerDashboardResponse>.Failure(
                new Error("BusinessId.Required", "BusinessId is required."));
        }

        var businessId = request.BusinessId.Value;

        var hasAdvancedAnalytics =
            await featureService.HasFeatureAsync(
                businessId,
                SellerPlanFeatureCodes.Dashboard.AdvancedAnalytics,
                cancellationToken);

        var hasTopProducts =
            await featureService.HasFeatureAsync(
                businessId,
                SellerPlanFeatureCodes.Dashboard.TopProducts,
                cancellationToken);

        var hasExtendedPeriod =
            await featureService.HasFeatureAsync(
                businessId,
                SellerPlanFeatureCodes.Dashboard.ExtendedPeriod,
                cancellationToken);

        var maxDays = hasExtendedPeriod ? 90 : 30;
        var days = Math.Clamp(request.Days, 7, maxDays);

        var now = DateTime.UtcNow;
        var fromUtc = now.Date.AddDays(-(days - 1));

        var data = await repository.GetDashboardAsync(
            fromUtc,
            now,
            businessId,
            cancellationToken);

        return Result<SellerDashboardResponse>.Success(
            new SellerDashboardResponse(
                data.TotalBusinesses,
                data.ActiveBusinesses,

                data.TotalProducts,
                data.ActiveProducts,
                data.LowStockProducts,
                data.OutOfStockProducts,

                data.TotalOrders,
                data.OrdersToday,

                data.PendingPaymentOrders,
                data.PaidOrders,
                data.ProcessingOrders,
                data.ShippedOrders,
                data.DeliveredOrders,
                data.CancelledOrders,

                data.TotalRevenue,
                data.RevenueToday,

                hasAdvancedAnalytics
                    ? data.RevenueTrend
                    : Array.Empty<SellerRevenuePoint>(),

                data.RecentOrders,

                hasTopProducts
                    ? data.TopProducts
                    : Array.Empty<SellerTopProduct>()));
    }
}
