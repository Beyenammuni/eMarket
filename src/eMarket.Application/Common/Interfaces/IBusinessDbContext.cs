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
    DbSet<SellerPlan> SellerPlans { get; }
    DbSet<SellerPlanSubscription> SellerPlanSubscriptions { get; }
    DbSet<SellerPlanPayment> SellerPlanPayments
  { get; }
    Task ExecuteInSerializableTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default);
}
