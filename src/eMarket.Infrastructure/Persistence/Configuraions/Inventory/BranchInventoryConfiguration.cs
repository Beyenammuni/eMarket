
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Variants;
using eMarket.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Inventory;

public sealed class BranchInventoryConfiguration
    : IEntityTypeConfiguration<BranchInventory>
{
    public void Configure(EntityTypeBuilder<BranchInventory> builder)
    {
        builder.ToTable("BranchInventories", table =>
        {
            table.HasCheckConstraint(
                "CK_BranchInventories_Quantity",
                "[Quantity] >= 0");

            table.HasCheckConstraint(
                "CK_BranchInventories_ReservedQuantity",
                "[ReservedQuantity] >= 0 AND [ReservedQuantity] <= [Quantity]");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.BranchId)
            .HasConversion(
                id => id.Value,
                value => BranchId.Create(value))
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value))
            .IsRequired();

        builder.Property(x => x.VariantId);

        builder.Property(x => x.Quantity)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ReservedQuantity)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensures that the selected variant belongs to the same product.
        builder.HasOne<ProductVariant>()
            .WithMany()
            .HasForeignKey(x => new { x.ProductId, x.VariantId })
            .HasPrincipalKey(x => new { x.ProductId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);

        // One product inventory record per branch when VariantId is null.
        builder.HasIndex(x => new { x.BranchId, x.ProductId })
            .IsUnique()
            .HasFilter("[VariantId] IS NULL");

        // One inventory record per variant per branch.
        builder.HasIndex(x => new { x.BranchId, x.VariantId })
            .IsUnique()
            .HasFilter("[VariantId] IS NOT NULL");

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.VariantId);
    }
}
