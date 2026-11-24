
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Payments;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Reports.Queries.GetAdvancedSalesReport;

internal sealed class GetAdvancedSalesReportQueryHandler
    : IRequestHandler<
        GetAdvancedSalesReportQuery,
        Result<AdvancedSalesReportResponse>>
{
    private readonly IReportsDbContext _dbContext;
    private readonly ISellerPlanFeatureService _featureService;

    public GetAdvancedSalesReportQueryHandler(
        IReportsDbContext dbContext,
        ISellerPlanFeatureService featureService)
    {
        _dbContext = dbContext;
        _featureService = featureService;
    }

    public async Task<Result<AdvancedSalesReportResponse>> Handle(
        GetAdvancedSalesReportQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BusinessId == Guid.Empty)
        {
            return Result<AdvancedSalesReportResponse>.Failure(
                new Error(
                    "Reports.InvalidBusinessId",
                    "Business ID is required."));
        }

        if (request.FromUtc >= request.ToUtc)
        {
            return Result<AdvancedSalesReportResponse>.Failure(
                new Error(
                    "Reports.InvalidDateRange",
                    "The start date must be earlier than the end date."));
        }

        var hasFeature = await _featureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Reports.AdvancedSales,
            cancellationToken);

        if (!hasFeature)
        {
            return Result<AdvancedSalesReportResponse>.Failure(
                new Error(
                    "Reports.ProFeatureRequired",
                    "Advanced sales reports require an active PRO subscription."));
        }


        var businessOrderIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.BusinessId == BusinessId.Create(request.BusinessId))
            .Select(order => order.Id.Value)
            .ToListAsync(cancellationToken);

        if (businessOrderIds.Count == 0)
        {
            return Result<AdvancedSalesReportResponse>.Success(
                new AdvancedSalesReportResponse(
                    0m, 0m, 0m, 0, 0,
                    Array.Empty<DailySalesReportItem>()));
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
                SalesDate = (payment.UpdatedAt ?? payment.CreatedAt).Date,
                TotalSales = payment.Amount.Amount,
                Commission = payment.PlatformCommissionAmount.Amount,
                SellerNet = payment.SellerAmount.Amount
            })
            .ToListAsync(cancellationToken);

        var sales = payments;


        var dailySales = sales
            .GroupBy(x => x.SalesDate)
            .OrderBy(x => x.Key)
            .Select(group => new DailySalesReportItem(
                group.Key,
                group.Sum(x => x.TotalSales),
                group.Sum(x => x.Commission),
                group.Sum(x => x.SellerNet),
                group.Select(x => x.OrderId).Distinct().Count()))
            .ToList();

        var response = new AdvancedSalesReportResponse(
            sales.Sum(x => x.TotalSales),
            sales.Sum(x => x.Commission),
            sales.Sum(x => x.SellerNet),
            sales.Select(x => x.OrderId).Distinct().Count(),
            sales.Count,
            dailySales);

        return Result<AdvancedSalesReportResponse>.Success(response);
    }
}
