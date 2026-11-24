using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository
    : Repository<Promotion, PromotionId>, IPromotionRepository
{
    private readonly AppDbContext _context;

    public PromotionRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Promotion?> GetByIdAsync(
        PromotionId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(
                p => p.Id == id &&
                     p.BusinessId == businessId,
                cancellationToken);
    }

    public async Task AddAsync(
        Promotion promotion,
        CancellationToken cancellationToken = default)
    {
        await _context.Promotions.AddAsync(
            promotion,
            cancellationToken);
    }

    public async Task<bool> HasOverlappingPromotionAsync(
        BusinessId businessId,
        IReadOnlyCollection<ProductId> productIds,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        PromotionId? excludedPromotionId = null,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return false;

        var query = _context.Promotions
            .Where(p =>
                p.BusinessId == businessId &&
                (p.Status == PromotionStatus.Active ||
                 p.Status == PromotionStatus.Scheduled ||
                 p.Status == PromotionStatus.Paused) &&
                p.StartsAtUtc < endsAtUtc &&
                p.EndsAtUtc > startsAtUtc);

        if (excludedPromotionId is not null)
        {
            query = query.Where(
                p => p.Id != excludedPromotionId);
        }

        return await query
            .SelectMany(p => p.Items)
            .AnyAsync(
                item => productIds.Contains(item.ProductId),
                cancellationToken);
    }
    public async Task<List<Promotion>> GetByBusinessIdAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .Where(p => p.BusinessId == businessId)
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Promotion>> GetActiveForCustomersAsync(
    DateTime nowUtc,
    CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p =>
                (p.Status == PromotionStatus.Active ||
                 p.Status == PromotionStatus.Scheduled) &&
                p.StartsAtUtc <= nowUtc &&
                p.EndsAtUtc > nowUtc)
            .Where(p =>
                _context.Businesses.Any(b =>
                    b.Id == p.BusinessId &&
                    b.Status == BusinessStatus.Active))
            .Where(p =>
                p.Items.Any(item =>
                    _context.Products.Any(product =>
                        product.Id == item.ProductId &&
                        product.BusinessId == p.BusinessId &&
                        product.Status == ProductStatus.Active &&
                        !product.IsDeleted)))
            .OrderBy(p => p.EndsAtUtc)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Promotion>> GetActiveForProductAsync(
    BusinessId businessId,
    ProductId productId,
    DateTime nowUtc,
    CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p =>
                p.BusinessId == businessId &&
                (p.Status == PromotionStatus.Active ||
                 p.Status == PromotionStatus.Scheduled) &&
                p.StartsAtUtc <= nowUtc &&
                p.EndsAtUtc > nowUtc)
            .Where(p =>
                p.Items.Any(item =>
                    item.ProductId == productId))
            .Where(p =>
                _context.Businesses.Any(b =>
                    b.Id == p.BusinessId &&
                    b.Status == BusinessStatus.Active))
            .OrderBy(p => p.EndsAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Promotion>> GetActiveForProductsAsync(
        BusinessId businessId,
        IReadOnlyCollection<ProductId> productIds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return [];

        return await _context.Promotions
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p =>
                p.BusinessId == businessId &&
                (p.Status == PromotionStatus.Active ||
                 p.Status == PromotionStatus.Scheduled) &&
                p.StartsAtUtc <= nowUtc &&
                p.EndsAtUtc > nowUtc)
            .Where(p =>
                _context.Businesses.Any(b =>
                    b.Id == p.BusinessId &&
                    b.Status == BusinessStatus.Active))
            .Where(p =>
    p.Items.Any(item =>
        productIds.Contains(item.ProductId) &&
        _context.Products.Any(product =>
            product.Id == item.ProductId &&
            product.BusinessId == businessId &&
            product.Status == ProductStatus.Active &&
            !product.IsDeleted)))
            .OrderBy(p => p.EndsAtUtc)
            .ToListAsync(cancellationToken);
    }

}

