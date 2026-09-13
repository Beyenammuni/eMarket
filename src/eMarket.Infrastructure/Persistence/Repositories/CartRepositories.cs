using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Cart cart,
        CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(
            cart,
            cancellationToken);
    }

    public async Task<Cart?> GetByIdAsync(
        CartId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Cart?> GetActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Status == CartStatus.Active,
                cancellationToken);
    }
}
