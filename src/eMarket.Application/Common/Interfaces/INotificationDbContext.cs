
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Notifications.Entities;
using eMarket.Domain.Sales.Orders;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<Notification> Notifications { get; }

    DbSet<Order> Orders { get; }

    DbSet<BusinessMember> BusinessMembers { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
