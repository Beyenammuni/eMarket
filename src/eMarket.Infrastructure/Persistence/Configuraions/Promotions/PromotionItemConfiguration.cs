
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Promotions;

public sealed class PromotionItemConfiguration
    : IEntityTypeConfiguration<PromotionItem>
{
    public void Configure(EntityTypeBuilder<PromotionItem> builder)
    {
        builder.ToTable("PromotionItems", table =>
        {
            table.HasCheckConstraint(
                "CK_PromotionItems_DiscountValue",
                "[DiscountValue] > 0 AND ([DiscountType] <> 1 OR [DiscountValue] < 100)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PromotionId)
            .HasConversion(
                id => id.Value,
                value => PromotionId.Create(value))
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value))
            .IsRequired();

        builder.Property(x => x.DiscountType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.PromotionId,
            x.ProductId
        }).IsUnique();

    }
}
