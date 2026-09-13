using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Admin.Queries.GetDashboard;

public sealed record GetAdminDashboardQuery(int Days = 30) : IRequest<Result<AdminDashboardResponse>>;

public sealed record AdminDashboardResponse(
    int TotalUsers,
    int ActiveUsers,
    int NewUsers,
    int TotalBusinesses,
    int ActiveBusinesses,
    int TotalProducts,
    int ActiveProducts,
    int LowStockProducts,
    int OutOfStockProducts,
    int TotalOrders,
    int OrdersToday,
    int PendingPaymentOrders,
    int PaidOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal TotalRevenue,
    decimal RevenueToday,
    int TotalPayments,
    int SuccessfulPayments,
    int FailedPayments,
    int ActiveSubscriptions,
    int CancelledSubscriptions,
    int SubscriptionsDueNext7Days,
    IReadOnlyList<AdminRevenuePoint> RevenueTrend,
    IReadOnlyList<AdminRecentOrder> RecentOrders,
    IReadOnlyList<AdminTopProduct> TopProducts);

internal sealed class GetAdminDashboardQueryHandler(IAdminDashboardRepository repository)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
{
    public async Task<Result<AdminDashboardResponse>>
        Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var days = Math.Clamp(request.Days, 7, 90);
        var now = DateTime.UtcNow;
        var data = await
            repository.GetDashboardAsync(
                now.Date.AddDays(-(days - 1)), now, cancellationToken);
        return Result<AdminDashboardResponse>.Success(new AdminDashboardResponse(
            data.TotalUsers,
            data.ActiveUsers,
            data.NewUsers,
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
            data.TotalPayments,
            data.SuccessfulPayments,
            data.FailedPayments,
            data.ActiveSubscriptions,
            data.CancelledSubscriptions,
            data.SubscriptionsDueNext7Days,
            data.RevenueTrend,
            data.RecentOrders,
            data.TopProducts));
    }
}
