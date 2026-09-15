using eMarket.Application.Businesses.Queries.GetSellerDashboard;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class SellerDashboardRepository(
    AppDbContext context,
    ICurrentUser currentUser)
    : ISellerDashboardRepository
{
    public async Task<SellerDashboardData> GetDashboardAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        if (userId is null)
            throw new UnauthorizedAccessException();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var businessIds = await context.BusinessMembers
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive)
            .Select(x => x.BusinessId)
            .Distinct()
            .ToListAsync(cancellationToken);


        var totalBusinesses = businessIds.Count;

        var activeBusinesses = await context.Businesses
            .AsNoTracking()
            .CountAsync(
                x =>
                    businessIds.Contains(x.Id) &&
                    x.Status == BusinessStatus.Active,
                cancellationToken);


        var sellerProducts = context.Products
            .AsNoTracking()
            .Where(x =>
                businessIds.Contains(x.BusinessId) &&
                !x.IsDeleted);

        var totalProducts =
            await sellerProducts.CountAsync(cancellationToken);

        var activeProducts =
            await sellerProducts.CountAsync(
                x => x.Status == ProductStatus.Active,
                cancellationToken);

        var lowStockProducts =
            await sellerProducts.CountAsync(
                x =>
                    x.StockQuantity > 0 &&
                    x.StockQuantity <= 5,
                cancellationToken);

        var outOfStockProducts =
            await sellerProducts.CountAsync(
                x => x.StockQuantity <= 0,
                cancellationToken);


        var sellerProductIds = await sellerProducts
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);


        var sellerOrders = context.Orders
            .AsNoTracking()
            .Where(order =>
                order.Items.Any(item =>
                    sellerProductIds.Contains(item.ProductId)));

        var totalOrders =
            await sellerOrders.CountAsync(cancellationToken);

        var ordersToday =
            await sellerOrders.CountAsync(
                x =>
                    x.CreatedAt >= today &&
                    x.CreatedAt < tomorrow,
                cancellationToken);

        var pendingPaymentOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.PendingPayment,
                cancellationToken);

        var paidOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.Paid,
                cancellationToken);

        var processingOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.Processing,
                cancellationToken);

        var shippedOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.Shipped,
                cancellationToken);

        var deliveredOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.Delivered,
                cancellationToken);

        var cancelledOrders =
            await sellerOrders.CountAsync(
                x => x.Status == OrderStatus.Cancelled,
                cancellationToken);


        var successfulPaymentOrderIds = await context.Payments
     .AsNoTracking()
     .Where(payment =>
         payment.Status == PaymentStatus.Succeeded)
     .Select(payment => payment.OrderId)
     .ToListAsync(cancellationToken);

        var successfulPaymentOrderIdSet =
            successfulPaymentOrderIds.ToHashSet();

        var sellerOrdersData = await sellerOrders
            .ToListAsync(cancellationToken);

        var successfulSellerOrders = sellerOrdersData
            .Where(order =>
                successfulPaymentOrderIdSet.Contains(order.Id.Value))
            .ToList();

        var sellerProductIdSet =
            sellerProductIds
                .Select(x => x.Value)
                .ToHashSet();

        var sellerRevenueItems = successfulSellerOrders
            .SelectMany(order =>
                order.Items
                    .Where(item =>
                        sellerProductIdSet.Contains(item.ProductId.Value))
                    .Select(item => new
                    {
                        order.Id,
                        order.CreatedAt,
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        Revenue =
                            item.UnitPrice.Amount * item.Quantity
                    }))
            .ToList();

        var totalRevenue =
            sellerRevenueItems.Sum(x => x.Revenue);

        var revenueToday =
            sellerRevenueItems
                .Where(x =>
                    x.CreatedAt >= today &&
                    x.CreatedAt < tomorrow)
                .Sum(x => x.Revenue);


        var revenueTrend = Enumerable
            .Range(
                0,
                (toUtc.Date - fromUtc.Date).Days + 1)
            .Select(i =>
            {
                var date = fromUtc.Date.AddDays(i);

                var revenue = sellerRevenueItems
                    .Where(x => x.CreatedAt.Date == date)
                    .Sum(x => x.Revenue);

                return new SellerRevenuePoint(
                    date,
                    revenue);
            })
            .ToList();


        var recentOrderData = await sellerOrders
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(order => new
            {
                OrderId = order.Id.Value,
                order.CreatedAt,
                Status = order.Status.ToString(),

                TotalAmount = order.Items
                    .Where(item =>
                        sellerProductIds.Contains(item.ProductId))
                    .Sum(item =>
                        item.UnitPrice.Amount * item.Quantity)
            })
            .ToListAsync(cancellationToken);

        var recentOrders = recentOrderData
            .Select(x =>
                new SellerRecentOrder(
                    x.OrderId,
                    x.TotalAmount,
                    x.Status,
                    x.CreatedAt))
            .ToList();


        var topProducts = sellerRevenueItems
            .GroupBy(x => new
            {
                x.ProductId,
                x.ProductName
            })
            .Select(g =>
                new SellerTopProduct(
                    g.Key.ProductId.Value,
                    g.Key.ProductName,
                    g.Sum(x => x.Quantity),
                    g.Sum(x => x.Revenue)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToList();


        return new SellerDashboardData(
            TotalBusinesses: totalBusinesses,
            ActiveBusinesses: activeBusinesses,

            TotalProducts: totalProducts,
            ActiveProducts: activeProducts,
            LowStockProducts: lowStockProducts,
            OutOfStockProducts: outOfStockProducts,

            TotalOrders: totalOrders,
            OrdersToday: ordersToday,

            PendingPaymentOrders: pendingPaymentOrders,
            PaidOrders: paidOrders,
            ProcessingOrders: processingOrders,
            ShippedOrders: shippedOrders,
            DeliveredOrders: deliveredOrders,
            CancelledOrders: cancelledOrders,

            TotalRevenue: totalRevenue,
            RevenueToday: revenueToday,

            RevenueTrend: revenueTrend,
            RecentOrders: recentOrders,
            TopProducts: topProducts);
    }
}
