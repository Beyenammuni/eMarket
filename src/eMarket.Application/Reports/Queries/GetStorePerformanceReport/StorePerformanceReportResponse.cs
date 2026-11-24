
namespace eMarket.Application.Reports.Queries.GetStorePerformanceReport;

public sealed record StorePerformanceReportResponse(
    decimal TotalSales,
    decimal PlatformCommission,
    decimal SellerNetAmount,
    decimal AverageOrderValue,
    int PaidOrdersCount,
    int TotalProductsSold
);
