using eMarket.Domain.Catalog.Categories;
using eMarket.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CategoryConfiguration
    : EntityConfiguration<Category, CategoryId>
{
    protected override CategoryId CreateId(Guid value)
        => CategoryId.Create(value);

    protected override void ConfigureEntity(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.Property(x => x.BusinessId)
            .IsRequired();

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.ParentCategoryId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null
                    ? null
                    : CategoryId.Create(value.Value))
            .IsRequired(false);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
