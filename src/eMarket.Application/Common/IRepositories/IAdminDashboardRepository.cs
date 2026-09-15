using eMarket.Domain.Identity;

namespace eMarket.Application.Common.IRepositories;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardData> GetDashboardAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}

public sealed record AdminDashboardData(
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

public sealed record AdminRevenuePoint(DateTime Date, decimal Revenue, int Orders);
public sealed record AdminTopProduct(Guid ProductId, string ProductName, int QuantitySold, decimal Revenue);
public sealed record AdminRecentOrder(Guid Id, Guid UserId, decimal Total, string Status, DateTime CreatedAt);
