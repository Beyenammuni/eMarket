using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value));

        builder.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                value => ProductName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                x => x.Value,
                value => ProductDescription.Create(value))
            .HasMaxLength(4000);

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            price.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.Sku)
            .HasConversion(
                x => x.Value,
                value => Sku.Create(value))
            .HasMaxLength(50);

        builder.HasIndex(x=>x.Sku)
            .IsUnique();

        builder.Property(x => x.CategoryId)
            .HasConversion(
                id => id.Value,
                value => CategoryId.Create(value));

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasOne<Category>()
    .WithMany()
    .HasForeignKey(x => x.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StockQuantity);

        builder.Property(x => x.CreatedAt);

        builder.Ignore(x => x.DomainEvents);
    }
}
