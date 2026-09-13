using eMarket.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface ISubscriptionDbContext
{
    DbSet<Subscription> Subscriptions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
