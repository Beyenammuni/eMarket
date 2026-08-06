using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence.Configurations;
using eMarket.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext ,IBusinessDbContext,
    ICatalogDbContext,
    IIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    public DbSet<BusinessMember> BusinessMembers => Set<BusinessMember>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
