
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Sales.Orders.Services;

public sealed class OrderStockReleaseService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStockReleaseService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
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

        var products = new List<(
            Domain.Sales.Orders.Entities.OrderItem Item,
            Product Product)>();

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                item.ProductId,
                cancellationToken);

            if (product is null)
                return Result.Failure(OrderErrors.NotFound);

            if (item.VariantId.HasValue &&
                !product.Variants.Any(v => v.Id == item.VariantId.Value))
            {
                return Result.Failure(OrderErrors.InvalidStatus);
            }

            products.Add((item, product));
        }

        var cancelResult = order.Cancel();

        if (cancelResult.IsFailure)
            return cancelResult;

        foreach (var entry in products)
        {
            if (entry.Item.VariantId.HasValue)
            {
                var variant = entry.Product.Variants.First(
                    v => v.Id == entry.Item.VariantId.Value);

                try
                {
                    variant.IncreaseStock(entry.Item.Quantity);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return Result.Failure(
                        ProductErrors.InvalidStock);
                }
                catch (OverflowException)
                {
                    return Result.Failure(
                        ProductErrors.InvalidStock);
                }
            }
            else
            {
                var stockResult = entry.Product.IncreaseStock(
                    entry.Item.Quantity);

                if (stockResult.IsFailure)
                    return stockResult;
            }
        }

        var releaseResult = order.MarkStockReleased();

        if (releaseResult.IsFailure)
            return releaseResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
