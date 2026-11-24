using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface IBusinessDbContext
{
    DbSet<Business> Businesses { get; }

    DbSet<BusinessMember> BusinessMembers { get; }
    DbSet<Branch> Branches { get; }
    DbSet<BusinessMemberPermission> BusinessMemberPermissions { get; }
    DbSet<BranchInventory> BranchInventories { get; }
    DbSet<BusinessActivityLog> BusinessActivityLogs { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
    DbSet<SellerPlan> SellerPlans { get; }
    DbSet<SellerPlanSubscription> SellerPlanSubscriptions { get; }
    DbSet<SellerPlanPayment> SellerPlanPayments
  { get; }
    DbSet<SellerPlanFeature> SellerPlanFeatures { get; }
    Task ExecuteInSerializableTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default);
}
