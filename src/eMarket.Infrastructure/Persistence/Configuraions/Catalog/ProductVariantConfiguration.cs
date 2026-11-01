
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Variants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductVariantConfiguration
    : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(50);

        builder.HasIndex(x => x.Sku)
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL");

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StockQuantity)
            .IsRequired();

        builder.ToTable("ProductVariants", table =>
        {
            table.HasCheckConstraint(
                "CK_ProductVariants_Price",
                "[Price] >= 0");

            table.HasCheckConstraint(
                "CK_ProductVariants_StockQuantity",
                "[StockQuantity] >= 0");
        });

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

    }
}
