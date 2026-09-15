using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface IBusinessDbContext
{
    DbSet<Business> Businesses { get; }

    DbSet<BusinessMember> BusinessMembers { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
