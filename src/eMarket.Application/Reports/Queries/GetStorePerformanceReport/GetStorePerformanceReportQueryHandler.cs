
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Payments;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Reports.Queries.GetStorePerformanceReport;

public sealed class GetStorePerformanceReportQueryHandler
    : IRequestHandler<GetStorePerformanceReportQuery, Result<StorePerformanceReportResponse>>
{
    private readonly IReportsDbContext _dbContext;
    private readonly ISellerPlanFeatureService _featureService;

    public GetStorePerformanceReportQueryHandler(
        IReportsDbContext dbContext,
        ISellerPlanFeatureService featureService)
    {
        _dbContext = dbContext;
        _featureService = featureService;
    }

    public async Task<Result<StorePerformanceReportResponse>> Handle(
        GetStorePerformanceReportQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BusinessId == Guid.Empty || request.FromUtc >= request.ToUtc)
            return Result<StorePerformanceReportResponse>.Failure(
    new Error(
        "Reports.InvalidDateRange",
        "The business ID or date range is invalid."));

        var hasFeature = await _featureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Reports.StorePerformance,
            cancellationToken);

        if (!hasFeature)
            return Result<StorePerformanceReportResponse>.Failure(
     new Error(
         "Reports.ProFeatureRequired",
         "This feature requires a PRO subscription."));
        var businessOrderIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.BusinessId == BusinessId.Create(request.BusinessId))
            .Select(order => order.Id.Value)
            .ToListAsync(cancellationToken);

        if (businessOrderIds.Count == 0)
        {
            return Result<StorePerformanceReportResponse>.Success(
                new StorePerformanceReportResponse(0m, 0m, 0m, 0m, 0, 0));
        }

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(payment =>
                payment.Status == PaymentStatus.Succeeded &&
                payment.OrderId.HasValue &&
                businessOrderIds.Contains(payment.OrderId.Value) &&
                (payment.UpdatedAt ?? payment.CreatedAt) >= request.FromUtc &&
                (payment.UpdatedAt ?? payment.CreatedAt) < request.ToUtc)
            .Select(payment => new
            {
                OrderId = payment.OrderId!.Value,
                TotalSales = payment.Amount.Amount,
                Commission = payment.PlatformCommissionAmount.Amount,
                SellerNet = payment.SellerAmount.Amount
            })
            .ToListAsync(cancellationToken);

        var paidOrdersCount = payments
            .Select(payment => payment.OrderId)
            .Distinct()
            .Count();

        var totalSales = payments.Sum(payment => payment.TotalSales);

        var paidOrderIds = payments
       .Select(payment => payment.OrderId)
       .Distinct()
       .ToList();
        var totalProductsSold = 0;

        if (paidOrderIds.Count > 0)
        {
            var businessOrders = await _dbContext.Orders
                .AsNoTracking()
                .Where(order =>
                    order.BusinessId == BusinessId.Create(request.BusinessId))
                .Select(order => new
                {
                    OrderId = order.Id.Value,
                    Items = order.Items
                })
                .ToListAsync(cancellationToken);

            totalProductsSold = businessOrders
                .Where(order => paidOrderIds.Contains(order.OrderId))
                .SelectMany(order => order.Items)
                .Sum(item => item.Quantity);
        }

        var response = new StorePerformanceReportResponse(
            totalSales,
            payments.Sum(payment => payment.Commission),
            payments.Sum(payment => payment.SellerNet),
            paidOrdersCount == 0 ? 0m : totalSales / paidOrdersCount,
            paidOrdersCount,
            totalProductsSold);

        return Result<StorePerformanceReportResponse>.Success(response);
    }
}
