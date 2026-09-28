using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;

namespace eMarket.Application.Common.IRepositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        CategoryId id,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        CategoryId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByNameAsync(
        CategoryName name,
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        CategoryName name,
        BusinessId businessId,
        CategoryId? excludedId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    Task<List<Category>> GetAllAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default);
}
