
namespace eMarket.Application.Reports.Queries.GetAdvancedSalesReport;

public sealed record AdvancedSalesReportResponse(
    decimal TotalSales,
    decimal PlatformCommission,
    decimal SellerNetAmount,
    int PaidOrdersCount,
    int SuccessfulPaymentsCount,
    IReadOnlyList<DailySalesReportItem> DailySales
);

public sealed record DailySalesReportItem(
    DateTime Date,
    decimal TotalSales,
    decimal PlatformCommission,
    decimal SellerNetAmount,
    int OrdersCount
);
