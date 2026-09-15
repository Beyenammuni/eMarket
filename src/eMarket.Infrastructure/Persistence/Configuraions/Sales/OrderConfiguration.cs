using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Sales;

public sealed class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // -------------------------
        // Order Id
        // -------------------------

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value))
            .ValueGeneratedNever();

        // -------------------------
        // User
        // -------------------------

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<eMarket.Domain.Identity.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Business
        // -------------------------

        builder.Property(x => x.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.Create(value))
            .IsRequired();

        builder.HasIndex(x => x.BusinessId);

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Status
        // -------------------------

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.Status);

        // -------------------------
        // Dates
        // -------------------------

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // -------------------------
        // Order Items
        // -------------------------

        builder.OwnsMany(
            x => x.Items,
            item =>
            {
                item.ToTable("OrderItems");

                item.WithOwner()
                    .HasForeignKey("OrderId");

                item.HasKey(
                    "OrderId",
                    "ProductId");

                item.Property(x => x.ProductId)
                    .HasConversion(
                        id => id.Value,
                        value => ProductId.Create(value))
                    .IsRequired();

                item.Property(x => x.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                item.OwnsOne(
                    x => x.UnitPrice,
                    money =>
                    {
                        money.Property(x => x.Amount)
                            .HasColumnName("UnitPrice")
                            .HasPrecision(18, 2)
                            .IsRequired();

                        money.Property(x => x.Currency)
                            .HasColumnName("Currency")
                            .HasConversion<int>()
                            .IsRequired();
                    });

                item.Property(x => x.Quantity)
                    .IsRequired();
            });

        // -------------------------
        // Delivery Address
        // -------------------------

        builder.OwnsOne(
            x => x.DeliveryAddress,
            address =>
            {
                address.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                address.Property(x => x.PhoneNumber)
                    .HasMaxLength(30)
                    .IsRequired();

                address.Property(x => x.AddressLine)
                    .HasMaxLength(500)
                    .IsRequired();

                address.Property(x => x.City)
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.District)
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.PostalCode)
                    .HasMaxLength(20)
                    .IsRequired();

                address.Property(x => x.Latitude)
                    .HasPrecision(9, 6)
                    .IsRequired();

                address.Property(x => x.Longitude)
                    .HasPrecision(9, 6)
                    .IsRequired();
            });

        builder.Ignore(x => x.DomainEvents);
    }
}
