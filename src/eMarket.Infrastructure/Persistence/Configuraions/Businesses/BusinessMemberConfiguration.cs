using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMarket.Infrastructure.Persistence.Configurations.Businesses;

public sealed class BusinessMemberConfiguration
    : IEntityTypeConfiguration<BusinessMember>
{
    public void Configure(
        EntityTypeBuilder<BusinessMember> builder)
    {
        builder.ToTable("BusinessMembers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                new StronglyTypedIdConverter<BusinessMemberId>(
                    BusinessMemberId.Create))
            .Metadata.SetValueComparer(
                new StronglyTypedIdComparer<BusinessMemberId>());

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.BusinessId)
            .HasConversion(
                new StronglyTypedIdConverter<BusinessId>(
                    BusinessId.Create))
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasConversion(
                new StronglyTypedIdConverter<UserId>(
                    UserId.Create))
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion(
                new EnumerationConverter<BusinessRole>())
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.UserId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive
        });

        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.Role,
            x.IsActive
        });
    }
}
