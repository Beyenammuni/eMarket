using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetSellerDashboard;

internal sealed class GetSellerDashboardQueryHandler(
    ISellerDashboardRepository repository)
    : IRequestHandler<
        GetSellerDashboardQuery,
        Result<SellerDashboardResponse>>
{
    public async Task<Result<SellerDashboardResponse>> Handle(
        GetSellerDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var days = Math.Clamp(request.Days, 7, 90);

        var now = DateTime.UtcNow;

        var fromUtc = now.Date.AddDays(-(days - 1));

        var data = await repository.GetDashboardAsync(
            fromUtc,
            now,
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

                data.RevenueTrend,
                data.RecentOrders,
                data.TopProducts));
    }
}
