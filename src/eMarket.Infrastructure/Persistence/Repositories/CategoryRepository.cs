using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository
    : Repository<Category, CategoryId>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(
            category,
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        CategoryName name,
        BusinessId businessId,
        CategoryId? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Where(c =>
                c.BusinessId == businessId.Value &&
                c.Name.Value == name.Value);

        if (excludedId is not null)
        {
            query = query.Where(c => c.Id != excludedId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        CategoryId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        CategoryId id,
        BusinessId businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(
                c =>
                    c.Id == id &&
                    c.BusinessId == businessId.Value,
                cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(
        CategoryName name,
        BusinessId businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(
                c =>
                    c.BusinessId == businessId.Value &&
                    c.Name.Value == name.Value,
                cancellationToken);
    }

    public async Task<List<Category>> GetAllAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId.Value)
            .OrderBy(c => c.Name.Value)
            .ToListAsync(cancellationToken);
    }
}
