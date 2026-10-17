using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class UserAddressRepository : IUserAddressRepository
{
    private readonly AppDbContext _context;

    public UserAddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAddress>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAddress?> GetByIdAsync(
        Guid id,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserAddress address,
        CancellationToken cancellationToken = default)
    {
        await _context.UserAddresses.AddAsync(address, cancellationToken);
    }

    public void Remove(UserAddress address)
    {
        _context.UserAddresses.Remove(address);
    }
}
