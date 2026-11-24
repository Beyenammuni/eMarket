using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Catalog.Products.Queries.GetAdvancedInventory;

public sealed class GetAdvancedInventoryQueryHandler
    : IRequestHandler<GetAdvancedInventoryQuery, Result<AdvancedInventoryResponse>>
{
    private const int LowStockThreshold = 5;

    private readonly IReportsDbContext _dbContext;
    private readonly ISellerPlanFeatureService _featureService;

    public GetAdvancedInventoryQueryHandler(
        IReportsDbContext dbContext,
        ISellerPlanFeatureService featureService)
    {
        _dbContext = dbContext;
        _featureService = featureService;
    }

    public async Task<Result<AdvancedInventoryResponse>> Handle(
        GetAdvancedInventoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BusinessId == Guid.Empty ||
            request.Page < 1 ||
            request.PageSize < 1 ||
            request.PageSize > 100)
        {
            return Result<AdvancedInventoryResponse>.Failure(
                new Error(
                    "Inventory.InvalidRequest",
                    "The business ID or pagination values are invalid."));
        }

        var hasFeature = await _featureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Inventory.AdvancedManagement,
            cancellationToken);

        if (!hasFeature)
        {
            return Result<AdvancedInventoryResponse>.Failure(
                new Error(
                    "Inventory.ProFeatureRequired",
                    "This feature requires a PRO subscription."));
        }

        var businessId = BusinessId.Create(request.BusinessId);

        var products = _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.BusinessId == businessId &&
                !product.IsDeleted);

        var totalProducts = await products.CountAsync(
            cancellationToken);

        var outOfStockCount = await products
            .CountAsync(
                product => product.StockQuantity == 0,
                cancellationToken);

        var lowStockCount = await products
            .CountAsync(
                product =>
                    product.StockQuantity > 0 &&
                    product.StockQuantity <= LowStockThreshold,
                cancellationToken);

        var totalInventoryValue = await products
            .SumAsync(
                product =>
                    product.Price.Amount * product.StockQuantity,
                cancellationToken);

        var totalCount = totalProducts;

        var items = await products
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(product => new InventoryProductItem(
                (string)(object)product.Name,
                (string)(object)product.Sku,
                product.StockQuantity,
                product.Price.Amount,
                product.Price.Amount * product.StockQuantity))
            .ToListAsync(cancellationToken);

        var response = new AdvancedInventoryResponse(
            totalProducts,
            outOfStockCount,
            lowStockCount,
            totalInventoryValue,
            request.Page,
            request.PageSize,
            totalCount,
            items);

        return Result<AdvancedInventoryResponse>.Success(response);
    }
}
