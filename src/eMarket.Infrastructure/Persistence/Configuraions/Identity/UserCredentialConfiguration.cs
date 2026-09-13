using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configuraions;

public sealed class UserCredentialConfiguration
    : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(
        EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("UserCredentials");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasConversion(
                new StronglyTypedIdConverter<UserId>(
                    UserId.Create))
            .Metadata.SetValueComparer(
                new StronglyTypedIdComparer<UserId>());

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserCredential>(
                x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
