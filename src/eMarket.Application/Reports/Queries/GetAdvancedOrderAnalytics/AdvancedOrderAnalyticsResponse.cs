
namespace eMarket.Application.Reports.Queries.GetAdvancedOrderAnalytics;

public sealed record AdvancedOrderAnalyticsResponse(
    int TotalOrders,
    int CancelledOrders,
    decimal CancellationRate,
    decimal AverageProductsPerOrder,
    IReadOnlyList<OrderStatusAnalyticsItem> OrdersByStatus,
    IReadOnlyList<TopOrderedProductItem> TopProducts
);

public sealed record OrderStatusAnalyticsItem(
    string Status,
    int Count
);

public sealed record TopOrderedProductItem(
    string ProductName,
    int TotalQuantity,
    int OrdersCount
);
