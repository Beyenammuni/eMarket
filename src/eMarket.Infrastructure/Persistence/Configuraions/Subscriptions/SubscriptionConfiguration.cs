using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
using eMarket.Domain.Subscriptions;
using eMarket.Domain.Subscriptions.Entities;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Subscriptions;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SubscriptionId
            .Create(value));
        builder.Property(x => x.UserId)
            .HasConversion(new StronglyTypedIdConverter<UserId>(UserId.Create))
            .Metadata.SetValueComparer(new StronglyTypedIdComparer<UserId>());
        builder.Property(x => x.BusinessId).HasConversion(id => id.Value, value => BusinessId.Create(value));
        builder.Property(x => x.DeliveryDay).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.NextDeliveryDate).IsRequired(); builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasIndex(x => new { x.UserId, x.Status }); builder.HasIndex(x => x.NextDeliveryDate);
        builder.Ignore(x => x.DomainEvents);
        builder.HasOne<eMarket.Domain.Identity.User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<eMarket.Domain.Businesses.Business>().WithMany().HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey("SubscriptionId").OnDelete(DeleteBehavior.Cascade);

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
    }
}

public sealed class SubscriptionItemConfiguration : IEntityTypeConfiguration<SubscriptionItem>
{
    public void Configure(EntityTypeBuilder<SubscriptionItem> builder)
    {
        builder.ToTable("SubscriptionItems"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ProductId).HasConversion(id => id.Value, value => ProductId.Create(value)).IsRequired();
        builder.Property(x => x.Quantity).IsRequired(); builder.HasIndex(x => new { x.ProductId });
        builder.Ignore(x => x.DomainEvents);
        builder.HasOne<eMarket.Domain.Catalog.Products.Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
