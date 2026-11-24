
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.ValueObjects;

namespace eMarket.Application.Common.IRepositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(
        PromotionId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Promotion promotion,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingPromotionAsync(
        BusinessId businessId,
        IReadOnlyCollection<ProductId> productIds,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        PromotionId? excludedPromotionId = null,
        CancellationToken cancellationToken = default);

    Task<List<Promotion>> GetByBusinessIdAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task<List<Promotion>> GetActiveForCustomersAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<List<Promotion>> GetActiveForProductAsync(
        BusinessId businessId,
        ProductId productId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<List<Promotion>> GetActiveForProductsAsync(
        BusinessId businessId,
        IReadOnlyCollection<ProductId> productIds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
