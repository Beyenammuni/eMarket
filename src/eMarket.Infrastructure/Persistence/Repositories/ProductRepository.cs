using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(
            product,
            cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        ProductId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        ProductId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.BusinessId == businessId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        BusinessId businessId,
        ProductName name,
        ProductId? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(x =>
                x.BusinessId == businessId &&
                x.Name == name);

        if (excludedId is not null)
        {
            query = query.Where(
                x => x.Id != excludedId);
        }

        return await query.AnyAsync(
            cancellationToken);
    }

    public async Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        BusinessId? businessId,
        CategoryId? categoryId,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (businessId is not null)
        {
            query = query.Where(
                x => x.BusinessId == businessId);
        }

        if (categoryId is not null)
        {
            query = query.Where(
                x => x.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(
                x => x.Name.Value.Contains(search) ||
                     x.Sku.Value.Contains(search));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "price" => sortDescending
                ? query.OrderByDescending(x => x.Price.Amount)
                : query.OrderBy(x => x.Price.Amount),

            "createdat" => sortDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            _ => query.OrderBy(x => x.Id)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
