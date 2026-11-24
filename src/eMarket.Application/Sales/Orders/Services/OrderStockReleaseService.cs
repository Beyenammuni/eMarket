
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Inventory.Entities;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Sales.Orders.Services;

public sealed class OrderStockReleaseService
{
    private readonly IBusinessDbContext _businessDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStockReleaseService(
        IBusinessDbContext businessDbContext,
        IUnitOfWork unitOfWork)
    {
        _businessDbContext = businessDbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ReleaseAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        if (order.Status != OrderStatus.PendingPayment ||
            order.StockReleased)
        {
            return Result.Failure(OrderErrors.InvalidStatus);
        }

        if (order.BranchId is null)
        {
            return Result.Failure(
                OrderErrors.InvalidStatus);
        }

        var releaseResult = Result.Failure(
            OrderErrors.InvalidStatus);

        await _businessDbContext.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                var inventories = new List<(
                    Domain.Sales.Orders.Entities.OrderItem Item,
                    BranchInventory Inventory)>();

                // Load and validate all inventory records first.
                foreach (var item in order.Items)
                {
                    var inventory = await _businessDbContext.BranchInventories
                        .FirstOrDefaultAsync(
                            x =>
                                x.BranchId == order.BranchId &&
                                x.ProductId == item.ProductId &&
                                x.VariantId == item.VariantId,
                            ct);

                    if (inventory is null)
                    {
                        releaseResult = Result.Failure(
                            OrderErrors.NotFound);
                        return;
                    }

                    inventories.Add((item, inventory));
                }

                var cancelResult = order.Cancel();

                if (cancelResult.IsFailure)
                {
                    releaseResult = cancelResult;
                    return;
                }

                // Restore stock to the same branch.
                foreach (var entry in inventories)
                {
                    entry.Inventory.IncreaseStock(
                        entry.Item.Quantity);
                }

                var markReleasedResult = order.MarkStockReleased();

                if (markReleasedResult.IsFailure)
                {
                    throw new InvalidOperationException(
                        "Failed to mark order stock as released.");
                }

                await _unitOfWork.SaveChangesAsync(ct);

                releaseResult = Result.Success();
            },
            cancellationToken);

        return releaseResult;
    }
}
