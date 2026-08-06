using eMarket.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
