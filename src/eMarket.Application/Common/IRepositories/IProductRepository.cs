using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;

namespace eMarket.Application.Common.IRepositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        ProductId id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        ProductId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        BusinessId businessId,
        ProductName name,
        ProductId? excludedId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        BusinessId? businessId,
        CategoryId? categoryId,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);
}
