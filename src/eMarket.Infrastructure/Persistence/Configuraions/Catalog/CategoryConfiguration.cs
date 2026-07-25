using eMarket.Domain.Catalog;
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

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
