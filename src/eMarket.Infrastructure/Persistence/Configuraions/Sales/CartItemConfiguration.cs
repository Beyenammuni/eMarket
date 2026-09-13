using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Carts.Entities;
using eMarket.Domain.Sales.Carts.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Sales;

public sealed class CartItemConfiguration
    : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => new
        {
            x.CartId,
            x.ProductId
        });

        builder.Property(x => x.CartId)
            .HasConversion(
                id => id.Value,
                value => CartId.Create(value))
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value))
            .IsRequired();

        builder.OwnsOne(
            x => x.UnitPrice,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("UnitPrice")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("Currency")
                    .HasConversion<int>()
                    .IsRequired();
            });

        builder.Property(x => x.Quantity)
            .HasConversion(
                quantity => quantity.Value,
                value => Quantity.Create(value))
            .IsRequired();

        builder.Ignore(x => x.TotalPrice);
    }
}
