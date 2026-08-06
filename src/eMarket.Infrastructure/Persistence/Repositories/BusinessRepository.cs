using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _context;

    public BusinessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Business business,
        CancellationToken cancellationToken = default)
    {
        await _context.Businesses.AddAsync(
            business,
            cancellationToken);
    }

    public async Task<Business?> GetByIdAsync(
        BusinessId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Businesses
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Business?> GetByNameAsync(
        BusinessName name,
        CancellationToken cancellationToken = default)
    {
        return await _context.Businesses
            .FirstOrDefaultAsync(
                x => x.Name.Value == name.Value,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        BusinessName name,
        CancellationToken cancellationToken = default)
    {
        return await _context.Businesses
            .AnyAsync(
                x => x.Name.Value == name.Value,
                cancellationToken);
    }

    public async Task<List<Business>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Businesses
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        BusinessName name,
        BusinessId? excludedId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Businesses.AsQueryable();

        query = query.Where(x => x.Name.Value == name.Value);

        if (excludedId is not null)
        {
            query = query.Where(x => x.Id != excludedId);
        }

        return await query.AnyAsync(cancellationToken);
    }
    public async Task<Business?> GetWithMembersAsync(
    BusinessId id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Businesses
            .Include(x => x.Members)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

}
