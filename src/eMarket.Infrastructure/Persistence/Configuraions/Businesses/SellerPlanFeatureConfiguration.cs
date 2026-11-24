using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class SellerPlanFeatureConfiguration
    : IEntityTypeConfiguration<SellerPlanFeature>
{
    public void Configure(EntityTypeBuilder<SellerPlanFeature> builder)
    {
        builder.ToTable("SellerPlanFeatures");

        builder.HasKey(x => new { x.SellerPlanId, x.Code });

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne<SellerPlan>()
            .WithMany()
            .HasForeignKey(x => x.SellerPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
