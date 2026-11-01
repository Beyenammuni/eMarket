using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configuraions.Catalog;

public sealed class ProductOptionGroupConfiguration
    : IEntityTypeConfiguration<ProductOptionGroup>
{
    public void Configure(EntityTypeBuilder<ProductOptionGroup> builder)
    {
        builder.ToTable("ProductOptionGroups", table =>
        {
            table.HasCheckConstraint(
                "CK_ProductOptionGroups_MinSelections",
                "[MinSelections] >= 0");

            table.HasCheckConstraint(
                "CK_ProductOptionGroups_MaxSelections",
                "[MaxSelections] >= 1");

            table.HasCheckConstraint(
                "CK_ProductOptionGroups_SelectionRange",
                "[MinSelections] <= [MaxSelections]");
        });

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

        builder.Property(x => x.IsRequired).IsRequired();
        builder.Property(x => x.MinSelections).IsRequired();
        builder.Property(x => x.MaxSelections).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.OptionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Options)
            .HasField("_options")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
