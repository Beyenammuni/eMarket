using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.Entities;
using eMarket.Domain.Subscriptions;
using eMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Admin;

public sealed class AdminDashboardRepository(AppDbContext context) : IAdminDashboardRepository
{
    public async Task<AdminDashboardData> GetDashboardAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var next7 = today.AddDays(7);

        var totalUsers = await context.Users.CountAsync(cancellationToken);
        var activeUsers = await context.Users.CountAsync(x => x.Status == UserStatus.Active, cancellationToken);
        var newUsers = await context.Users.CountAsync(x => x.CreatedAt >= fromUtc && x.CreatedAt <= toUtc, cancellationToken);

        var totalBusinesses = await context.Businesses.CountAsync(cancellationToken);
        var activeBusinesses = await context.Businesses.CountAsync(x => x.Status == BusinessStatus.Active, cancellationToken);

        var totalProducts = await context.Products
            .CountAsync(x =>
        !x.IsDeleted, cancellationToken);
        var activeProducts = await context.Products
            .CountAsync(x => !x.IsDeleted && x.Status ==
            ProductStatus.Active, cancellationToken);
        var lowStockProducts = await
            context.Products.CountAsync(x => !x.IsDeleted && x.StockQuantity >
            0 && x.StockQuantity <= 5, cancellationToken);
        var outOfStockProducts = await
            context.Products.CountAsync(x => !x.IsDeleted && x.StockQuantity
            <= 0, cancellationToken);

        var totalOrders = await context.Orders.CountAsync(cancellationToken);
        var ordersToday = await context.Orders.CountAsync(x => x.CreatedAt >= today, cancellationToken);
        var pending = await context.Orders.CountAsync(x => x.Status == OrderStatus.PendingPayment, cancellationToken);
        var paid = await context.Orders.CountAsync(x => x.Status == OrderStatus.Paid, cancellationToken);
        var processing = await context.Orders.CountAsync(x => x.Status == OrderStatus.Processing, cancellationToken);
        var shipped = await context.Orders.CountAsync(x => x.Status == OrderStatus.Shipped, cancellationToken);
        var delivered = await context.Orders.CountAsync(x => x.Status == OrderStatus.Delivered, cancellationToken);
        var cancelled = await context.Orders.CountAsync(x => x.Status == OrderStatus.Cancelled, cancellationToken);

        var successfulPayments = await context.Payments.CountAsync(x => x.Status == PaymentStatus.Succeeded, cancellationToken);
        var failedPayments = await context.Payments.CountAsync(x => x.Status == PaymentStatus.Failed, cancellationToken);
        var totalPayments = await context.Payments.CountAsync(cancellationToken);
        var totalRevenue = await context.Payments.Where(x => x.Status == PaymentStatus.Succeeded).SumAsync(x => x.Amount.Amount, cancellationToken);
        var revenueToday = await context.Payments.Where(x => x.Status == PaymentStatus.Succeeded && x.CreatedAt >= today).SumAsync(x => x.Amount.Amount, cancellationToken);

        var activeSubscriptions = await context.Subscriptions.CountAsync(x => x.Status == SubscriptionStatus.Active, cancellationToken);
        var cancelledSubscriptions = await context.Subscriptions.CountAsync(x => x.Status == SubscriptionStatus.Cancelled, cancellationToken);
        var dueNext7 = await context.Subscriptions.CountAsync(x => x.Status == SubscriptionStatus.Active && x.NextDeliveryDate >= today && x.NextDeliveryDate < next7, cancellationToken);

        var payments = await context.Payments.AsNoTracking()
            .Where(x => x.Status == PaymentStatus.Succeeded && x.CreatedAt >= fromUtc && x.CreatedAt <= toUtc)
            .Select(x => new { x.CreatedAt, Amount = x.Amount.Amount })
            .ToListAsync(cancellationToken);

        var ordersForTrend = await context.Orders.AsNoTracking()
            .Where(x => x.CreatedAt >= fromUtc && x.CreatedAt <= toUtc)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var revenueTrend = Enumerable.Range(0, (toUtc.Date - fromUtc.Date).Days + 1)
            .Select(i => fromUtc.Date.AddDays(i))
            .Select(date => new AdminRevenuePoint(
                date,
                payments.Where(x => x.CreatedAt.Date == date).Sum(x => x.Amount),
                ordersForTrend.Count(x => x.Date == date)))
            .ToList();

        var recentOrders = await context.Orders.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new AdminRecentOrder(x.Id.Value, x.UserId.Value, x.TotalAmount, x.Status.ToString(), x.CreatedAt))
            .ToListAsync(cancellationToken);

        var topProductItems = await context.Orders
    .AsNoTracking()
    .SelectMany(order => order.Items)
    .Select(item => new
    {
        item.ProductId,
        item.ProductName,
        item.Quantity,
        UnitPrice = item.UnitPrice.Amount
    })
    .ToListAsync(cancellationToken);

        var topProducts = topProductItems
            .GroupBy(x => new
            {
                x.ProductId,
                x.ProductName
            })
            .Select(g => new AdminTopProduct(
                g.Key.ProductId.Value,
                g.Key.ProductName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.UnitPrice * x.Quantity)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToList();

        return new AdminDashboardData(totalUsers, activeUsers, newUsers, totalBusinesses, activeBusinesses,
            totalProducts, activeProducts, lowStockProducts, outOfStockProducts, totalOrders, ordersToday,
            pending, paid, processing, shipped, delivered, cancelled, totalRevenue, revenueToday,
            totalPayments, successfulPayments, failedPayments, activeSubscriptions, cancelledSubscriptions,
            dueNext7, revenueTrend, recentOrders, topProducts);
    }
}
