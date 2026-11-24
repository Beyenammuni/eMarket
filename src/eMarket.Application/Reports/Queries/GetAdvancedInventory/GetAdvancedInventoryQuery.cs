using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetAdvancedInventory;

public sealed record GetAdvancedInventoryQuery(
    Guid BusinessId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<AdvancedInventoryResponse>>;

public sealed record AdvancedInventoryResponse(
    int TotalProducts,
    int OutOfStockCount,
    int LowStockCount,
    decimal TotalInventoryValue,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<InventoryProductItem> Items
);

public sealed record InventoryProductItem(
    string Name,
    string Sku,
    int StockQuantity,
    decimal UnitPrice,
    decimal StockValue
);
