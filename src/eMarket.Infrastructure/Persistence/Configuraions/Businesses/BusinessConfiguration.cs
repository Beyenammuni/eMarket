using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Infrastructure.Persistence.Configurations.Base;
using eMarket.Infrastructure.Persistence.Converters;
using eMarket.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class BusinessConfiguration
    : EntityConfiguration<Business, BusinessId>
{
    protected override BusinessId CreateId(Guid value)
        => BusinessId.Create(value);

    protected override void ConfigureEntity(
        EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(b => b.Type)
     .HasConversion(
         v => v.Name,
         v => Enumeration.FromName<BusinessType>(v))
     .HasMaxLength(50)
     .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Navigation(x => x.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
