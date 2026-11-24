
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Reports.Queries.GetAdvancedOrderAnalytics;

public sealed class GetAdvancedOrderAnalyticsQueryHandler
    : IRequestHandler<GetAdvancedOrderAnalyticsQuery, Result<AdvancedOrderAnalyticsResponse>>
{
    private readonly IReportsDbContext _dbContext;
    private readonly ISellerPlanFeatureService _featureService;

    public GetAdvancedOrderAnalyticsQueryHandler(
        IReportsDbContext dbContext,
        ISellerPlanFeatureService featureService)
    {
        _dbContext = dbContext;
        _featureService = featureService;
    }

    public async Task<Result<AdvancedOrderAnalyticsResponse>> Handle(
        GetAdvancedOrderAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BusinessId == Guid.Empty || request.FromUtc >= request.ToUtc)
        {
            return Result<AdvancedOrderAnalyticsResponse>.Failure(
                new Error(
                    "Orders.InvalidDateRange",
                    "The business ID or date range is invalid."));
        }

        var hasFeature = await _featureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Orders.AdvancedAnalytics,
            cancellationToken);

        if (!hasFeature)
        {
            return Result<AdvancedOrderAnalyticsResponse>.Failure(
                new Error(
                    "Orders.ProFeatureRequired",
                    "This feature requires a PRO subscription."));
        }

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.BusinessId == BusinessId.Create(request.BusinessId) &&
                order.CreatedAt >= request.FromUtc &&
                order.CreatedAt < request.ToUtc)
            .Select(order => new
            {
                order.Status,
                ProductsCount = order.Items.Sum(item => item.Quantity),
                Items = order.Items.Select(item => new
                {
                    item.ProductName,
                    item.Quantity
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;

        var cancelledOrders = orders.Count(
            order => order.Status == OrderStatus.Cancelled);

        var ordersByStatus = orders
            .GroupBy(order => order.Status)
            .OrderBy(group => group.Key)
            .Select(group => new OrderStatusAnalyticsItem(
                group.Key.ToString(),
                group.Count()))
            .ToList();

        var topProducts = orders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.ProductName)
            .Select(group => new TopOrderedProductItem(
                group.Key,
                group.Sum(item => item.Quantity),
                group.Count()))
            .OrderByDescending(product => product.TotalQuantity)
            .Take(5)
            .ToList();

        var response = new AdvancedOrderAnalyticsResponse(
            totalOrders,
            cancelledOrders,
            totalOrders == 0
                ? 0m
                : Math.Round((decimal)cancelledOrders / totalOrders * 100, 2),
            totalOrders == 0
                ? 0m
                : Math.Round(
                    (decimal)orders.Sum(order => order.ProductsCount) / totalOrders,
                    2),
            ordersByStatus,
            topProducts);

        return Result<AdvancedOrderAnalyticsResponse>.Success(response);
    }
}
