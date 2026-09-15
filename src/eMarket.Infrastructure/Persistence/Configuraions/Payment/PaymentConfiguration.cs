using eMarket.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace eMarket.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(
        EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => PaymentId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PlatformCommissionRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasMaxLength(50);

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(200);

        builder.OwnsOne(
            x => x.Amount,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("Currency")
                    .HasConversion<int>()
                    .IsRequired();
            });

        builder.OwnsOne(
            x => x.PlatformCommissionAmount,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("PlatformCommissionAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("PlatformCommissionCurrency")
                    .HasConversion<int>()
                    .IsRequired();
            });

        builder.OwnsOne(
            x => x.SellerAmount,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("SellerAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("SellerCurrency")
                    .HasConversion<int>()
                    .IsRequired();
            });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Ignore(x => x.DomainEvents);
    }
}

