using eMarket.Domain.Catalog;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface ICatalogDbContext
{
    DbSet<Category> Categories { get; }

    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
