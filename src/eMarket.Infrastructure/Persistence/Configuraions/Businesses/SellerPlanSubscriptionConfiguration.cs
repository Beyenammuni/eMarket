
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class SellerPlanSubscriptionConfiguration
    : IEntityTypeConfiguration<SellerPlanSubscription>
{
    public void Configure(
        EntityTypeBuilder<SellerPlanSubscription> builder)
    {
        builder.ToTable("SellerPlanSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.Create(value))
            .IsRequired();

        builder.Property(x => x.SellerPlanId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentToken)
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.BusinessId);

        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.Status
        });

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SellerPlan>()
            .WithMany()
            .HasForeignKey(x => x.SellerPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
