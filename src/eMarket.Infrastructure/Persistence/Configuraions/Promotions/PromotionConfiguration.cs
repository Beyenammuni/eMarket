
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Promotions;

public sealed class PromotionConfiguration
    : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions", table =>
        {
            table.HasCheckConstraint(
                "CK_Promotions_DateRange",
                "[EndsAtUtc] > [StartsAtUtc]");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => PromotionId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.Create(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.StartsAtUtc)
            .IsRequired();

        builder.Property(x => x.EndsAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
     .WithOne()
     .HasForeignKey(x => x.PromotionId)
     .IsRequired()
     .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.BusinessId);

        builder.HasIndex(x => new
        {
            x.Status,
            x.StartsAtUtc,
            x.EndsAtUtc
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
