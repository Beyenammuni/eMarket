
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Common.Interfaces;

public interface IReportsDbContext
{
    DbSet<Order> Orders { get; }

    DbSet<Payment> Payments { get; }
    DbSet<Product> Products { get; }

}
