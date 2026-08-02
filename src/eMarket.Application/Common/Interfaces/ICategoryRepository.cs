using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;

namespace eMarket.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        CategoryId id,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByNameAsync(
        CategoryName name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        CategoryName name,
           CategoryId? excludedId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(
    CancellationToken cancellationToken = default);
}
