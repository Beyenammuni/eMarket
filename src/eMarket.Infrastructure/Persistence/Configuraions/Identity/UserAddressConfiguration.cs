using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserAddressConfiguration
    : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(
                new StronglyTypedIdConverter<UserId>(UserId.Create))
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AddressLine)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.District)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Neighborhood)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Street)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.BuildingNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ApartmentNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);


        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.HasOne<User>()
    .WithMany()
    .HasForeignKey(x => x.UserId)
    .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsDefault
        });
    }
}
