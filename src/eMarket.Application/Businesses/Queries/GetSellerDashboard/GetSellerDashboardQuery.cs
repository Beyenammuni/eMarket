using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetSellerDashboard;

public sealed record GetSellerDashboardQuery(
    int Days = 30)
    : IRequest<Result<SellerDashboardResponse>>;

public sealed record SellerDashboardResponse(
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

public sealed record SellerRevenuePoint(
    DateTime Date,
    decimal Revenue);

public sealed record SellerRecentOrder(
    Guid OrderId,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);

public sealed record SellerTopProduct(
    Guid ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue);
