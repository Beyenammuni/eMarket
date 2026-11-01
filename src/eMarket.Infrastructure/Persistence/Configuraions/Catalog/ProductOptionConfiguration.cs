
using eMarket.Domain.Catalog.Products.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductOptionConfiguration
    : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("ProductOptions", table =>
        {
            table.HasCheckConstraint(
                "CK_ProductOptions_AdditionalPrice",
                "[AdditionalPrice] >= 0");

            table.HasCheckConstraint(
                "CK_ProductOptions_DisplayOrder",
                "[DisplayOrder] >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OptionGroupId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AdditionalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.OptionGroupId, x.DisplayOrder });
    }
}
