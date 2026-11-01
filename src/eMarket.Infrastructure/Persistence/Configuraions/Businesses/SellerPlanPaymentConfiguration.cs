
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class SellerPlanPaymentConfiguration
    : IEntityTypeConfiguration<SellerPlanPayment>
{
    public void Configure(EntityTypeBuilder<SellerPlanPayment> builder)
    {
        builder.ToTable("SellerPlanPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.Create(value))
            .IsRequired();

        builder.Property(x => x.SellerPlanId).IsRequired();
        builder.Property(x => x.SellerPlanSubscriptionId).IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ProviderToken).HasMaxLength(200);
        builder.Property(x => x.ProviderPaymentId).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.BusinessId);
        builder.HasIndex(x => x.ProviderToken)
            .IsUnique()
            .HasFilter("[ProviderToken] IS NOT NULL");
        builder.HasIndex(x => x.SellerPlanSubscriptionId);

        builder.HasOne<SellerPlan>()
            .WithMany()
            .HasForeignKey(x => x.SellerPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SellerPlanSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SellerPlanSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
