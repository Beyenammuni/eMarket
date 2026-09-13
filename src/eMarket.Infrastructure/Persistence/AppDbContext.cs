using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity.Entities;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Subscriptions;
using eMarket.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using eMarket.SharedKernel.Exceptions;

namespace eMarket.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext ,IBusinessDbContext,
    ICatalogDbContext,
    IIdentityDbContext, ISubscriptionDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
 
    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BusinessMember> BusinessMembers => Set<BusinessMember>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Domain.Identity.Entities.UserCredential> UserCredentials => Set<Domain.Identity.Entities.UserCredential>();
    public DbSet<Domain.Identity.Entities.PasswordResetToken> PasswordResetTokens => Set<Domain.Identity.Entities.PasswordResetToken>();
    public DbSet<Domain.Identity.Entities.UserRoleAssignment> UserRoleAssignments => Set<Domain.Identity.Entities.UserRoleAssignment>();

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
