using eMarket.Application.Businesses.Queries.GetSellerDashboard;

namespace eMarket.Application.Common.IRepositories;

public interface ISellerDashboardRepository
{
    Task<SellerDashboardData> GetDashboardAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}

public sealed record SellerDashboardData(
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

    IReadOnlyList<SellerRevenuePoint> RevenueTrend,
    IReadOnlyList<SellerRecentOrder> RecentOrders,
    IReadOnlyList<SellerTopProduct> TopProducts);
