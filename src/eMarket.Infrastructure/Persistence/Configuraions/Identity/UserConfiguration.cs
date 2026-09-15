using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence.Converters;
using eMarket.SharedKernel.Common;
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
            .HasConversion(
                new StronglyTypedIdConverter<UserId>(UserId.Create))
            .Metadata.SetValueComparer(
                new StronglyTypedIdComparer<UserId>());

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique();


        builder.OwnsOne(x => x.FullName, name =>
        {
            name.Property(x => x.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(x => x.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();

            name.WithOwner();
        });


        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.WithOwner();
        });

   

        builder.OwnsOne(x => x.PhoneNumber, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20)
                .IsRequired();

            phone.WithOwner();
        });



        builder.Property(x => x.Status)
            .HasConversion(
                new EnumerationConverter<UserStatus>())
            .IsRequired();


        builder.Property(x => x.EmailVerified)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastLoginAt);


        builder.Ignore(x => x.DomainEvents);


        builder.Ignore(x => x.Roles);
    }
}
