using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence.Converters;
using EMarket.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
    .HasConversion(new StronglyTypedIdConverter<UserId>(UserId.Create))
    .Metadata.SetValueComparer(new StronglyTypedIdComparer<UserId>());

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(x => x.FullName, name =>
        {
            name.Property(p => p.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(p => p.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(p => p.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
        });
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.OwnsOne(x => x.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20)
                .IsRequired();
        });
        builder.Ignore(x => x.Roles);
        builder.Property(x => x.Status)
    .HasConversion(new EnumerationConverter<UserStatus>());

        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.EmailVerified);

        builder.Property(x => x.CreatedAt);

        builder.Property(x => x.LastLoginAt);
    }
}
